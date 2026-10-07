namespace clock.Services;

public enum RunState { Stopped, Running, Paused, Finished }

public sealed class TimerService
{
    private readonly List<TimerEntry> items = [];
    private int nextNumber;
    public event Action? Changed;
    private void OnChanged() => Changed?.Invoke();
    public IReadOnlyList<TimerEntry> Items { get; }
    public TimerService() => Items = items.AsReadOnly();

    public TimerEntry Add(TimeSpan duration)
    {
        var timer = new TimerEntry(nextNumber + 1, duration);
        nextNumber++;
        items.Add(timer);
        timer.Changed += OnChanged;
        OnChanged();
        return timer;
    }
    public void Remove(Guid id)
    {
        var timer = items.Find(item => item.Id == id);
        if (timer is null) return;
        timer.Changed -= OnChanged;
        timer.Reset();
        items.Remove(timer);
        OnChanged();
    }
    public void Restore(IEnumerable<TimerSnapshot> saved)
    {
        Clear();
        foreach (var snapshot in saved)
        {
            var timer = new TimerEntry(snapshot);
            timer.Changed += OnChanged;
            items.Add(timer);
        }
        nextNumber = items.Count == 0 ? 0 : items.Max(item => item.Number);
    }
    public void Refresh()
    {
        foreach (var timer in items) timer.Refresh();
    }
    public void Clear()
    {
        foreach (var timer in items) { timer.Changed -= OnChanged; timer.Reset(); }
        items.Clear();
        OnChanged();
    }
}
