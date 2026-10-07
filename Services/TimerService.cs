namespace clock.Services;

public enum RunState { Stopped, Running, Paused, Finished }

public sealed class TimerService
{
    private readonly System.Diagnostics.Stopwatch elapsed = new();
    public RunState State { get; private set; }
    public TimeSpan Duration { get; private set; }
    public TimeSpan Remaining => TimeSpan.FromTicks(Math.Max(0, (Duration - elapsed.Elapsed).Ticks));
    public void Start(TimeSpan duration)
    {
        if (State is RunState.Running or RunState.Paused || duration <= TimeSpan.Zero) return;
        Duration = duration;
        elapsed.Restart();
        State = RunState.Running;
    }
    public void Pause()
    {
        Refresh();
        if (State != RunState.Running) return;
        elapsed.Stop();
        State = RunState.Paused;
    }
    public void Resume()
    {
        if (State != RunState.Paused) return;
        elapsed.Start();
        State = RunState.Running;
    }
    public void Reset()
    {
        elapsed.Reset();
        State = RunState.Stopped;
    }
    public void Refresh()
    {
        if (State != RunState.Running || Remaining > TimeSpan.Zero) return;
        elapsed.Stop();
        State = RunState.Finished;
    }
}
