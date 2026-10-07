using System.Text.Json;

namespace clock.Services;

// One coalescing writer; snapshots are captured on the UI dispatcher, file I/O runs off it.
public sealed class TimeStateStore(string path, AlarmService alarms, TimerService timers)
{
    private readonly object gate = new();
    private TimeStateSnapshot? pending;
    private Task writer = Task.CompletedTask;
    private bool observing;
    private bool canSave = true;
    public string? Error { get; private set; }

    public async Task InitializeAsync()
    {
        if (observing) return;
        try
        {
            if (File.Exists(path))
            {
                var text = await File.ReadAllTextAsync(path);
                var state = JsonSerializer.Deserialize(text, TimeStateJsonContext.Default.TimeStateSnapshot)
                    ?? throw new InvalidDataException("Saved state is empty.");
                Validate(state);
                alarms.Restore(state.Alarms);
                timers.Restore(state.Timers);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            Error = "Could not restore saved alarms and timers: " + ex.Message;
            try
            {
                if (File.Exists(path)) File.Move(path, path + ".invalid-" + Guid.NewGuid().ToString("N"));
            }
            catch (Exception backupError) when (backupError is IOException or UnauthorizedAccessException)
            {
                canSave = false;
                Error += " The original file was preserved; saving is unavailable.";
            }
        }
        alarms.Changed += RequestSave;
        timers.Changed += RequestSave;
        observing = true;
        RequestSave(); // Persist elapsed events discovered during restoration.
    }

    private static void Validate(TimeStateSnapshot state)
    {
        if (state.Version != 1 || state.Alarms is null || state.Timers is null ||
            state.Alarms.Length > 10000 || state.Timers.Length > 10000)
            throw new InvalidDataException("Unsupported saved state.");
        var ids = new HashSet<Guid>();
        var numbers = new HashSet<int>();
        foreach (var alarm in state.Alarms)
            if (alarm is null || alarm.Id == Guid.Empty || !ids.Add(alarm.Id) || alarm.Number <= 0 ||
                alarm.Number == int.MaxValue || !numbers.Add(alarm.Number) || !Enum.IsDefined(alarm.State) || !AlarmSchedule.IsValid(alarm.Days))
                throw new InvalidDataException("Invalid saved alarm.");
        numbers.Clear();
        foreach (var timer in state.Timers)
            if (timer is null || timer.Id == Guid.Empty || !ids.Add(timer.Id) || timer.Number <= 0 ||
                timer.Number == int.MaxValue || !numbers.Add(timer.Number) || !Enum.IsDefined(timer.State) ||
                timer.DurationTicks <= 0 || timer.DurationTicks > TimeSpan.FromHours(1000).Ticks || timer.RemainingTicks < 0 || timer.RemainingTicks > timer.DurationTicks ||
                (timer.State == RunState.Running && timer.DeadlineUtc is null))
                throw new InvalidDataException("Invalid saved timer.");
    }

    public void RequestSave()
    {
        if (!observing || !canSave) return;
        var snapshot = new TimeStateSnapshot(1,
            alarms.Items.Select(item => item.Snapshot()).ToArray(),
            timers.Items.Select(item => item.Snapshot()).ToArray());
        lock (gate)
        {
            pending = snapshot;
            if (writer.IsCompleted) writer = Task.Run(WritePendingAsync);
        }
    }

    private async Task WritePendingAsync()
    {
        while (true)
        {
            TimeStateSnapshot snapshot;
            lock (gate)
            {
                if (pending is null)
                {
                    // Clear under the same lock to avoid losing a concurrently queued save.
                    writer = Task.CompletedTask;
                    return;
                }
                snapshot = pending;
                pending = null;
            }
            var temporary = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
                var json = JsonSerializer.Serialize(snapshot, TimeStateJsonContext.Default.TimeStateSnapshot);
                await File.WriteAllTextAsync(temporary, json);
                File.Move(temporary, path, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Error = "Could not save alarms and timers: " + ex.Message;
            }
        }
    }

    public async Task FlushAsync()
    {
        RequestSave();
        Task task;
        lock (gate) task = writer;
        await task;
    }

    public void StopObserving()
    {
        if (!observing) return;
        observing = false;
        alarms.Changed -= RequestSave;
        timers.Changed -= RequestSave;
    }
}
