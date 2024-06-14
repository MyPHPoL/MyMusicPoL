using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Filters;

public sealed class FftFilter : IFilter
{
    private readonly NWaves.Transforms.RealFft64 fft;
    private readonly double[] re;
    private readonly double[] im;
    private readonly double[] inBuffer;
    private readonly double[] outBuffer;

    public FftFilter(int length)
    {
        var fftSize = length * 4;
        fft = new(fftSize);
        //fftBuffer = new double[length];
        re = new double[fftSize];
        im = new double[fftSize];
        inBuffer = new double[fftSize];
        outBuffer = new double[fftSize - 1];
    }

    public double[] process(double[] buffer)
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            inBuffer[i] = buffer[i];
        }
        for (var i = buffer.Length; i < inBuffer.Length; i++)
        {
            inBuffer[i] = 0;
        }

        fft.Direct(inBuffer, re, im);

        for (var i = 0; i < re.Length; i++)
        {
            inBuffer[i] = Math.Sqrt(re[i] * re[i] + im[i] * im[i]);
        }
        // normalize
        for (var i = 0; i < buffer.Length; i++)
        {
            inBuffer[i] /= (buffer.Length);
        }

        for (var i = 0; i < outBuffer.Length; i++)
        {
            outBuffer[i] = inBuffer[i + 1];
        }
        return outBuffer;
    }
}
