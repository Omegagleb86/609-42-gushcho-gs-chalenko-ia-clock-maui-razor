namespace clock.Services;

[Flags]
public enum AlarmDays
{
    None = 0, Sunday = 1, Monday = 2, Tuesday = 4, Wednesday = 8,
    Thursday = 16, Friday = 32, Saturday = 64, All = 127
}

public sealed record AlarmDayChoice(AlarmDays Day, string Label);

public static class AlarmSchedule
{
    public static IReadOnlyList<AlarmDayChoice> Choices { get; } = Array.AsReadOnly(new[]
    {
        new AlarmDayChoice(AlarmDays.Monday, "Mon"), new AlarmDayChoice(AlarmDays.Tuesday, "Tue"),
        new AlarmDayChoice(AlarmDays.Wednesday, "Wed"), new AlarmDayChoice(AlarmDays.Thursday, "Thu"),
        new AlarmDayChoice(AlarmDays.Friday, "Fri"), new AlarmDayChoice(AlarmDays.Saturday, "Sat"),
        new AlarmDayChoice(AlarmDays.Sunday, "Sun")
    });

    public static bool IsValid(AlarmDays days) => (days & ~AlarmDays.All) == 0;
    public static DateTime Next(TimeOnly time, AlarmDays days, DateTime after)
    {
        if (!IsValid(days)) throw new ArgumentOutOfRangeException(nameof(days));
        for (int offset = 0; offset <= 7; offset++)
        {
            var target = after.Date.AddDays(offset).Add(time.ToTimeSpan());
            var day = (AlarmDays)(1 << (int)target.DayOfWeek);
            if (target > after && (days == AlarmDays.None || (days & day) != 0)) return target;
        }
        throw new InvalidOperationException("No next alarm occurrence found.");
    }
    public static string Describe(AlarmDays days) => days == AlarmDays.None ? "One time" :
        string.Join(", ", Choices.Where(choice => (days & choice.Day) != 0).Select(choice => choice.Label));
}
