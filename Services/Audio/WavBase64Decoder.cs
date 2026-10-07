namespace clock.Services.Audio;

public sealed class WavBase64Decoder
{
    public byte[] Decode(string text)
    {
        if (text.Length > WavData.MaxTextBytes)
            throw new InvalidDataException("The Base64 text is too large (maximum 36 MB).");
        byte[] wav;
        try { wav = Convert.FromBase64String(text.TrimStart('\uFEFF')); }
        catch (FormatException ex) { throw new InvalidDataException("The file does not contain valid Base64 text.", ex); }
        WavData.Validate(wav);
        return wav;
    }
}
