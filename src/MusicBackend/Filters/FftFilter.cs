using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Filters;
public sealed class FftFilter : IFilter
{
	FftSharp.Window? Window;
	public double[] process(double[] buffer)
	{
		if (Window is null)
		{
			Window = new FftSharp.Windows.Hanning();
		}
		Window.ApplyInPlace(buffer);
		var fftArray = FftSharp.FFT.Forward(buffer);
		return FftSharp.FFT.Magnitude(fftArray,true);
	}
}
