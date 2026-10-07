namespace clock.Services;

public static class TimeDisplay
{
    public static string Format(TimeSpan value, bool roundUp = false)
    {
        var seconds = (long)(roundUp ? Math.Ceiling(value.TotalSeconds) : Math.Floor(value.TotalSeconds));
        seconds = Math.Max(0, seconds);
        return $"{seconds / 3600:00}:{seconds / 60 % 60:00}:{seconds % 60:00}";
    }
}
