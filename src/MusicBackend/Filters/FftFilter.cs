using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Filters;
public sealed class FftFilter : IFilter
{
	private readonly NWaves.Transforms.RealFft64 fft;
	private readonly double[] re;
	private readonly double[] im;
	public FftFilter(int length)
	{
		fft = new(length);
		//fftBuffer = new double[length];
		re = new double[length];
		im = new double[length];
	}
	public double[] process(double[] buffer)
	{
		fft.Direct(buffer,re,im);

		
		// normalize real and imaginary parts
		//for (var i = 0; i < re.Length; i++)
		//{
		//	re[i] /= re.Length;
		//	im[i] /= re.Length;
		//}
		
		for (var i = 0; i < re.Length; i++)
		{
			buffer[i] = Math.Sqrt(re[i] * re[i] + im[i] * im[i]);
		}
		// normalize
		for (var i = 0; i < buffer.Length; i++)
		{
			buffer[i] /= (buffer.Length);
		}
		return buffer;
	}
}
