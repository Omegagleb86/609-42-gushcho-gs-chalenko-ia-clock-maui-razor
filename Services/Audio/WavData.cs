using System.Buffers.Binary;

namespace clock.Services.Audio;

public static class WavData
{
    public const int MaxBytes = 25 * 1024 * 1024;
    public const int MaxTextBytes = 36 * 1024 * 1024;

    public static void Validate(ReadOnlySpan<byte> data)
    {
        if (data.Length > MaxBytes) throw new InvalidDataException("WAV files must be at most 25 MB.");
        if (data.Length < 12 || !data[..4].SequenceEqual("RIFF"u8) || !data.Slice(8, 4).SequenceEqual("WAVE"u8))
            throw new InvalidDataException("This is not a RIFF/WAVE file. Renaming MP3 to .wav does not convert it.");
        long end = 8L + BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4, 4));
        if (end != data.Length) throw new InvalidDataException("The WAV file is incomplete or has an invalid length.");
        bool formatFound = false, samplesFound = false;
        long position = 12;
        while (position + 8 <= end)
        {
            var chunk = data.Slice((int)position, 8);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(chunk[4..]);
            long next = position + 8 + size;
            if (next > end) throw new InvalidDataException("A WAV chunk is truncated.");
            if (chunk[..4].SequenceEqual("fmt "u8))
            {
                if (size < 16) throw new InvalidDataException("The WAV format chunk is invalid.");
                var format = data.Slice((int)position + 8, 16);
                var encoding = BinaryPrimitives.ReadUInt16LittleEndian(format);
                if (encoding is not (1 or 3 or 65534))
                    throw new InvalidDataException("Use PCM or floating-point WAV audio.");
                if (BinaryPrimitives.ReadUInt16LittleEndian(format[2..]) == 0 ||
                    BinaryPrimitives.ReadUInt32LittleEndian(format[4..]) == 0)
                    throw new InvalidDataException("The WAV channel count or sample rate is invalid.");
                formatFound = true;
            }
            if (chunk[..4].SequenceEqual("data"u8) && size > 0) samplesFound = true;
            position = next + (size & 1);
        }
        if (!formatFound || !samplesFound || position != end)
            throw new InvalidDataException("The WAV file must contain complete format and audio data chunks.");
    }
}
