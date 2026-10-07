namespace clock.Services;

public static class TimeDisplay
{
    public static string Format(TimeSpan value, bool roundUp = false)
    {
        var seconds = (long)(roundUp ? Math.Ceiling(value.TotalSeconds) : Math.Floor(value.TotalSeconds));
        seconds = Math.Max(0, seconds);
        return $"{seconds / 3600:00}:{seconds / 60 % 60:00}:{seconds % 60:00}";
    }
    public static string FormatUntil(TimeSpan value)
    {
        var minutes = (long)Math.Ceiling(Math.Max(0, value.TotalMinutes));
        return $"{minutes / 1440:00} д. {minutes / 60 % 24:00} ч. {minutes % 60:00} мин.";
    }
    public static string FormatPrecise(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        return $"{Format(value)}.{value.Milliseconds:000}";
    }
}
