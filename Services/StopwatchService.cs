namespace clock.Services;

public sealed record StopwatchMark(int Number, TimeSpan Elapsed);

public sealed class StopwatchService
{
    private readonly System.Diagnostics.Stopwatch elapsed = new();
    private readonly List<StopwatchMark> marks = [];
    public IReadOnlyList<StopwatchMark> Marks { get; }
    public StopwatchService() => Marks = marks.AsReadOnly();
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
    public void Capture()
    {
        if (State != RunState.Running) return;
        marks.Add(new StopwatchMark(marks.Count + 1, elapsed.Elapsed));
    }
    public void Reset()
    {
        elapsed.Reset();
        marks.Clear();
        State = RunState.Stopped;
    }
}
