namespace clock.Services.Audio;

public enum AlertSound { Alarm, Timer }

public sealed class BundledAudioService(WavBase64Decoder decoder)
{
    public static string FileName(AlertSound sound) => sound switch
    {
        AlertSound.Alarm => "clock.base64.txt",
        AlertSound.Timer => "timer.base64.txt",
        _ => throw new ArgumentOutOfRangeException(nameof(sound))
    };

    public async Task<byte[]> DecodeAsync(AlertSound sound, CancellationToken cancellationToken = default)
    {
        using var stream = await FileSystem.Current.OpenAppPackageFileAsync("Audio/" + FileName(sound));
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return await Task.Run(() => decoder.Decode(text), cancellationToken);
    }
}
