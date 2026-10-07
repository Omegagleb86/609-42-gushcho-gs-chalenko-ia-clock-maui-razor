namespace clock.Services;

public sealed class AlarmEntry
{
    public Guid Id { get; } = Guid.NewGuid();
    public event Action? Changed;
    public int Number { get; }
    public TimeOnly Time { get; private set; }
    public AlarmState State { get; private set; }
    public AlarmDays Days { get; private set; }
    public DateTime ScheduledAt { get; private set; }

    internal AlarmEntry(int number, TimeOnly time, AlarmDays days = AlarmDays.None)
    {
        if (!AlarmSchedule.IsValid(days)) throw new ArgumentOutOfRangeException(nameof(days));
        Number = number;
        Days = days;
        Set(time);
    }

    internal AlarmEntry(AlarmSnapshot saved)
    {
        Id = saved.Id; Number = saved.Number; Time = saved.Time;
        Days = saved.Days;
        State = saved.State; ScheduledAt = saved.ScheduledUtc.LocalDateTime;
        Refresh();
    }
    public AlarmSnapshot Snapshot() => new(Id, Number, Time, State, new DateTimeOffset(ScheduledAt).ToUniversalTime(), Days);

    public void Set(TimeOnly time)
    {
        Time = time;
        ScheduledAt = AlarmSchedule.Next(time, Days, DateTime.Now);
        State = AlarmState.Active;
        Changed?.Invoke();
    }
    public void SetDays(AlarmDays days)
    {
        if (!AlarmSchedule.IsValid(days)) throw new ArgumentOutOfRangeException(nameof(days));
        if (Days == days) return;
        Days = days;
        if (State != AlarmState.Triggered)
            ScheduledAt = AlarmSchedule.Next(Time, Days, DateTime.Now);
        Changed?.Invoke();
    }
    public void Dismiss()
    {
        if (State != AlarmState.Triggered) return;
        if (Days == AlarmDays.None) Disable();
        else Set(Time);
    }
    public void Enable() => Set(Time);
    public void Disable()
    {
        if (State == AlarmState.Disabled) return;
        State = AlarmState.Disabled;
        Changed?.Invoke();
    }
    public void Refresh()
    {
        if (State == AlarmState.Active && DateTime.Now >= ScheduledAt)
        {
            State = AlarmState.Triggered;
            Changed?.Invoke();
        }
    }
}
