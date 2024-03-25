using MusicBackend.Filters;
using MusicBackend.Interfaces;

namespace MusicBackend.Model;
public sealed class Visualizer : IDisposable
{

	double[] buffer;
	double[] sampleBuffer;
	double[] processedBuffer;
	double[] smoothedArray;
	bool shouldUpdate;
	object _lockObject = new();
	private IFilter windowFunction;
	private IFilter fftFilter;
	private IFilter[] filters;
	private IFilter smoothingFilter;
	private IFilter interpolateFilter;
	private double power;
	private double smoothPower;

	public Visualizer()
	{
		PlayerModel.Instance.audioWrapper.SamplesAccumulated += SamplesNotify;
		var bufferLength = AudioWrapper.BUFFER_SIZE;
		CreateFilters(bufferLength);
		shouldUpdate = true;
	}

	private void SamplesNotify(float[] samples)
	{
		lock(_lockObject)
		{
			if (buffer is null)
			{
				buffer = new double[samples.Length];
			}
			for (int i = 0; i < samples.Length; i++)
			{
				buffer[i] = samples[i];
			}
			shouldUpdate = true;
		}
	}

	private void CreateFilters(int length)
	{
		buffer = new double[length];
		sampleBuffer = new double[length];
		processedBuffer = new double[length];
		smoothedArray = new double[length];
		windowFunction = new WindowFunction(new DefaultWindow(length));
		fftFilter = new FftFilter(length);
		filters =
		[
			new MovingAverage(),
			new LogScale(1.02,length),
			new LerpFilter(10, length),
			new MovingAverage(),
			new LogBinFilter(1.02,5),
			//new ClampBinFilter(0.30),
		];
		interpolateFilter = new LerpFilter(2, length);
		smoothingFilter = new SmoothingFilter(0.4);
	}

	/**
	 * Returns updated buffer for display and power of lower frequencies
	 */
	public (double[],double) Update()
	{
		if (shouldUpdate)
		{
			lock (_lockObject)
			{
				for (int i = 0; i != buffer.Length; ++i)
				{
					sampleBuffer[i] = buffer[i];
				}
				shouldUpdate = false;
			}
			processedBuffer = windowFunction.process(sampleBuffer);
			processedBuffer = fftFilter.process(processedBuffer);
			foreach (var filter in filters)
			{
				processedBuffer = filter.process(processedBuffer);
			}
			power = CalculatePower();
			//processedBuffer = interpolateFilter.process(processedBuffer);
			if (processedBuffer.Length != smoothedArray.Length)
			{
				smoothedArray = new double[processedBuffer.Length];
				//for (int i = 0; i < processedBuffer.Length; i++)
				//{
				//	smoothedArray[i] = processedBuffer[i];
				//}
			}
			smoothingFilter.refresh(processedBuffer);
			// smoothedArray = processedBuffer;

			//return (processedBuffer, );
		}

		double delta = power - smoothPower;
		smoothPower += delta * 0.5;

		smoothedArray = smoothingFilter.process(smoothedArray);
		
		var smoothedBuffer = smoothedArray;
		//var smoothedBuffer = processedBuffer;

		return (smoothedBuffer, smoothPower);
	}

	double CalculatePower()
	{
		double power = 0;
		for (int i = 0; i < 70; i++)
		{
			power += processedBuffer[i];
		}
		return 5 * (power / 140);
	}

	public void Dispose()
	{
		PlayerModel.Instance.audioWrapper.SamplesAccumulated -= SamplesNotify;
		GC.SuppressFinalize(this);
	}
}
