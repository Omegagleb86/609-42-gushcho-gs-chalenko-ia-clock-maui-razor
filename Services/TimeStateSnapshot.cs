namespace clock.Services;

public sealed record AlarmSnapshot(Guid Id, int Number, TimeOnly Time, AlarmState State, DateTimeOffset ScheduledUtc, AlarmDays Days = AlarmDays.None);
public sealed record TimerSnapshot(Guid Id, int Number, RunState State, long DurationTicks, long RemainingTicks, DateTimeOffset? DeadlineUtc);
public sealed record TimeStateSnapshot(int Version, AlarmSnapshot[] Alarms, TimerSnapshot[] Timers);
