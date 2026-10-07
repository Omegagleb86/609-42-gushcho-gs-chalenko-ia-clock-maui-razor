namespace clock.Services;

public enum AlarmState { Disabled, Active, Triggered }

public sealed class AlarmService
{
    public AlarmState State { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public void Set(TimeOnly time)
    {
        var now = DateTime.Now;
        var target = now.Date.Add(time.ToTimeSpan());
        ScheduledAt = target <= now ? target.AddDays(1) : target;
        State = AlarmState.Active;
    }
    public void Disable() => State = AlarmState.Disabled;
    public void Refresh()
    {
        if (State == AlarmState.Active && DateTime.Now >= ScheduledAt)
            State = AlarmState.Triggered;
    }
}
