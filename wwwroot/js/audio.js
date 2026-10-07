// Audio is decoded from C#-supplied WAV bytes; no Base64 conversion lives here.
export function createAlertPlayer() {
    const context = new (window.AudioContext || window.webkitAudioContext)();
    const buffers = new Map();
    const sources = new Map();
    let desired = new Map();
    let disposed = false;
    let enabled = false;
    let generation = 0;
    function stop(name) {
        const playing = sources.get(name);
        if (playing) {
            playing.source.stop();
            playing.source.disconnect();
            sources.delete(name);
        }
    }
    function stopAll() {
        for (const id of [...sources.keys()]) stop(id);
    }
    function apply() {
        if (disposed) return;
        for (const [id, playing] of sources) {
            if (!enabled || !desired.has(id) || desired.get(id) !== playing.sound) stop(id);
        }
        if (!enabled || context.state !== "running") return;
        for (const [id, sound] of desired) {
            if (sources.has(id)) continue;
            const buffer = buffers.get(sound);
            if (!buffer) continue;
            const source = context.createBufferSource();
            source.buffer = buffer;
            source.loop = true;
            source.connect(context.destination);
            sources.set(id, { source, sound });
            source.start();
        }
    }
    return {
        async prepare(name, stream) {
            const bytes = await stream.arrayBuffer();
            const buffer = await context.decodeAudioData(bytes);
            if (!disposed) { buffers.set(name, buffer); apply(); }
        },
        async enable() {
            if (disposed) return false;
            const request = ++generation;
            await context.resume();
            if (disposed || request !== generation) return false;
            enabled = context.state === "running";
            apply();
            return enabled;
        },
        disable() {
            generation++;
            enabled = false;
            stopAll();
        },
        sync(alerts) {
            desired = new Map(alerts.map(alert => [alert.id, alert.sound]));
            apply();
        },
        async dispose() {
            disposed = true;
            generation++;
            enabled = false;
            stopAll();
            buffers.clear();
            desired.clear();
            await context.close();
        }
    };
}
