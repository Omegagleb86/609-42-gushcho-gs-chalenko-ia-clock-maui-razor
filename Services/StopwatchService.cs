namespace clock.Services;

public sealed class StopwatchService
{
    private readonly System.Diagnostics.Stopwatch elapsed = new();
    public RunState State { get; private set; }
    public TimeSpan Elapsed => elapsed.Elapsed;
    public void Start()
    {
        if (State != RunState.Stopped) return;
        elapsed.Start();
        State = RunState.Running;
    }
    public void Pause()
    {
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
}
