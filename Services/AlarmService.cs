namespace clock.Services;

public enum AlarmState { Disabled, Active, Triggered }

public sealed class AlarmService
{
    private readonly List<AlarmEntry> items = [];
    private int nextNumber;
    public event Action? Changed;
    private void OnChanged() => Changed?.Invoke();
    public IReadOnlyList<AlarmEntry> Items { get; }
    public AlarmEntry? NextAlarm => items.Where(item => item.State == AlarmState.Active).OrderBy(item => item.ScheduledAt).FirstOrDefault();
    public AlarmService() => Items = items.AsReadOnly();

    public AlarmEntry Add(TimeOnly time, AlarmDays days = AlarmDays.None)
    {
        var alarm = new AlarmEntry(nextNumber + 1, time, days);
        nextNumber++;
        items.Add(alarm);
        alarm.Changed += OnChanged;
        OnChanged();
        return alarm;
    }
    public void Remove(Guid id)
    {
        var alarm = items.Find(item => item.Id == id);
        if (alarm is null) return;
        alarm.Changed -= OnChanged;
        alarm.Disable();
        items.Remove(alarm);
        OnChanged();
    }
    public void Restore(IEnumerable<AlarmSnapshot> saved)
    {
        Clear();
        foreach (var snapshot in saved)
        {
            var alarm = new AlarmEntry(snapshot);
            alarm.Changed += OnChanged;
            items.Add(alarm);
        }
        nextNumber = items.Count == 0 ? 0 : items.Max(item => item.Number);
    }
    public void Refresh()
    {
        foreach (var alarm in items) alarm.Refresh();
    }
    public void Clear()
    {
        foreach (var alarm in items) { alarm.Changed -= OnChanged; alarm.Disable(); }
        items.Clear();
        OnChanged();
    }
}
