using MusicBackend.Interfaces;

namespace MusicBackend.Filters;

public class MovingAverage : IFilter
{
    public double[] process(double[] buffer)
    {
        for (int i = 2; i < buffer.Length - 2; i++)
        {
            buffer[i] =
                0.1F * (buffer[i - 2] + buffer[i + 2])
                + 0.2F * (buffer[i - 1] + buffer[i + 1])
                + 0.4F * (buffer[i]);
        }
        return buffer;
    }
}
