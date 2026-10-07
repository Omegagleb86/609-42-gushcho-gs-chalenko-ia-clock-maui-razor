namespace clock.Services;

public sealed class TimerEntry
{
    private readonly System.Diagnostics.Stopwatch elapsed = new();
    private TimeSpan remainingAtStart;
    public DateTimeOffset? DeadlineUtc { get; private set; }
    public event Action? Changed;
    public Guid Id { get; } = Guid.NewGuid();
    public int Number { get; }
    public RunState State { get; private set; }
    public TimeSpan Duration { get; }
    public TimeSpan Remaining => TimeSpan.FromTicks(Math.Max(0, (remainingAtStart - elapsed.Elapsed).Ticks));

    internal TimerEntry(int number, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        Number = number;
        Duration = duration;
        remainingAtStart = duration;
    }
    internal TimerEntry(TimerSnapshot saved)
    {
        Id = saved.Id; Number = saved.Number;
        Duration = TimeSpan.FromTicks(saved.DurationTicks);
        State = saved.State;
        remainingAtStart = State switch
        {
            RunState.Stopped => Duration,
            RunState.Finished => TimeSpan.Zero,
            _ => TimeSpan.FromTicks(saved.RemainingTicks)
        };
        if (State == RunState.Running)
        {
            DeadlineUtc = saved.DeadlineUtc;
            remainingAtStart = DeadlineUtc!.Value - DateTimeOffset.UtcNow;
            if (remainingAtStart <= TimeSpan.Zero)
            {
                remainingAtStart = TimeSpan.Zero; State = RunState.Finished; DeadlineUtc = null;
            }
            else
            {
                if (remainingAtStart > Duration)
                {
                    remainingAtStart = Duration;
                    DeadlineUtc = DateTimeOffset.UtcNow + Duration;
                }
                elapsed.Start();
            }
        }
    }
    public TimerSnapshot Snapshot() => new(Id, Number, State, Duration.Ticks,
        State == RunState.Running ? 0 : Remaining.Ticks, DeadlineUtc);

    public void Start()
    {
        if (State is RunState.Running or RunState.Paused) return;
        remainingAtStart = Duration;
        DeadlineUtc = DateTimeOffset.UtcNow + Duration;
        elapsed.Restart();
        State = RunState.Running;
        Changed?.Invoke();
    }
    public void Pause()
    {
        Refresh();
        if (State != RunState.Running) return;
        remainingAtStart = Remaining;
        elapsed.Reset();
        DeadlineUtc = null;
        State = RunState.Paused;
        Changed?.Invoke();
    }
    public void Resume()
    {
        if (State != RunState.Paused) return;
        DeadlineUtc = DateTimeOffset.UtcNow + remainingAtStart;
        elapsed.Start();
        State = RunState.Running;
        Changed?.Invoke();
    }
    public void Reset()
    {
        elapsed.Reset();
        remainingAtStart = Duration;
        DeadlineUtc = null;
        State = RunState.Stopped;
        Changed?.Invoke();
    }
    public void Refresh()
    {
        if (State != RunState.Running || Remaining > TimeSpan.Zero) return;
        elapsed.Reset();
        remainingAtStart = TimeSpan.Zero;
        DeadlineUtc = null;
        State = RunState.Finished;
        Changed?.Invoke();
    }
}
