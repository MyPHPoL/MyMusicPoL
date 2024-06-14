using NAudio.Wave;

namespace MusicBackend.Utils;

internal class SampleAccumulator : ISampleProvider
{
    public event EventHandler<float[]> SamplesAccumulated;
    private readonly ISampleProvider Source;
    private readonly float[] Buffer;
    int readCursor;
    int channels;
    public WaveFormat WaveFormat => Source.WaveFormat;

    public SampleAccumulator(ISampleProvider source, int bufferLength)
    {
        Source = source;
        Buffer = new float[bufferLength];
        readCursor = 0;
        channels = source.WaveFormat.Channels;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var readCount = Source.Read(buffer, offset, count);
        if (channels == 1)
        {
            for (int i = 0; i != readCount; ++i)
            {
                Append(buffer[i + offset]);
            }
        }
        else if (channels == 2)
        {
            for (int i = 0; i != readCount; i += 2)
            {
                Append((buffer[i + offset] + buffer[i + offset + 1]) / 2);
            }
        }

        return readCount;
    }

    private void Append(float v)
    {
        Buffer[readCursor] = v;
        readCursor++;
        if (readCursor == Buffer.Length)
        {
            SamplesAccumulated?.Invoke(this, Buffer);
            readCursor = 0;
        }
    }
}
