using Microsoft.JSInterop;

namespace clock.Services.Audio;

public sealed record AlertPlayback(string Id, string Sound);

public sealed class AlertAudioService(IJSRuntime js, BundledAudioService audio) : IAsyncDisposable
{
    private IJSObjectReference? module;
    private IJSObjectReference? player;
    private Task? initialization;
    private bool disposed;
    private AlertPlayback[]? lastState;
    public bool Ready { get; private set; }
    public bool Enabled { get; private set; }
    public bool Changing { get; private set; }
    public string? Error { get; private set; }

    public Task InitializeAsync() => initialization ??= InitializeCoreAsync();

    private async Task InitializeCoreAsync()
    {
        try
        {
            module = await js.InvokeAsync<IJSObjectReference>("import", "./js/audio.js");
            player = await module.InvokeAsync<IJSObjectReference>("createAlertPlayer");
            foreach (var sound in new[] { AlertSound.Alarm, AlertSound.Timer })
            {
                var bytes = await audio.DecodeAsync(sound);
                using var stream = new MemoryStream(bytes);
                using var reference = new DotNetStreamReference(stream);
                await player.InvokeVoidAsync("prepare", sound == AlertSound.Alarm ? "alarm" : "timer", reference);
            }
            Ready = true;
        }
        catch (Exception ex) when (ex is IOException or JSException or InvalidOperationException)
        {
            Error = "Could not prepare alert sounds: " + ex.Message;
        }
    }

    public Task ToggleAsync() => Enabled ? DisableAsync() : EnableAsync();

    public async Task EnableAsync()
    {
        if (disposed || !Ready || player is null || Changing) return;
        Changing = true;
        try
        {
            Enabled = await player.InvokeAsync<bool>("enable");
            Error = Enabled ? null : "Sound is blocked. Press Enable sounds again.";
        }
        catch (JSException ex) { Error = "Could not enable sound: " + ex.Message; }
        finally { Changing = false; }
    }

    public async Task DisableAsync()
    {
        if (disposed || !Ready || player is null || Changing) return;
        Changing = true;
        try
        {
            await player.InvokeVoidAsync("disable");
            Enabled = false;
            Error = null;
        }
        catch (JSException ex) { Error = "Could not disable sound: " + ex.Message; }
        finally { Changing = false; }
    }

    public async Task SyncAsync(IEnumerable<AlarmEntry> alarms, IEnumerable<TimerEntry> timers)
    {
        if (disposed || !Ready || player is null) return;
        var alerts = alarms.Where(item => item.State == AlarmState.Triggered)
            .Select(item => new AlertPlayback($"alarm-{item.Id}", "alarm"))
            .Concat(timers.Where(item => item.State == RunState.Finished)
                .Select(item => new AlertPlayback($"timer-{item.Id}", "timer")))
            .ToArray();
        if (lastState is not null && lastState.SequenceEqual(alerts)) return;
        try
        {
            // Wrap the array as a single JS argument, rather than a params array.
            await player.InvokeVoidAsync("sync", new object[] { alerts });
            lastState = alerts;
        }
        catch (JSException ex) { Error = "Could not play alert sound: " + ex.Message; }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        if (initialization is not null) await initialization;
        try
        {
            if (player is not null)
            {
                await player.InvokeVoidAsync("dispose");
                await player.DisposeAsync();
            }
            if (module is not null) await module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
    }
}
