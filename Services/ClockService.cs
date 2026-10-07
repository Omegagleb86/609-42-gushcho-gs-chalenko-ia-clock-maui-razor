namespace clock.Services;

public sealed class ClockService
{
    public DateTime Now { get; private set; } = DateTime.Now;
    public void Refresh() => Now = DateTime.Now;
}
