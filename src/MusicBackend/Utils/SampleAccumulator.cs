using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Utils;
internal class SampleAccumulator : ISampleProvider
{
	public event EventHandler<float[]> SamplesAggregated;
	private readonly ISampleProvider Source;
	private readonly float[] Buffer;
	int readCursor;
	public WaveFormat WaveFormat => Source.WaveFormat;

	public SampleAccumulator(ISampleProvider source, int bufferLength)
	{
		Source = source;
		Buffer = new float[bufferLength];
		readCursor = 0;
	}

	public int Read(float[] buffer, int offset, int count)
	{
		var readCount = Source.Read(buffer, offset, count);
		for (int i = 0; i != readCount; ++i)
		{
			Append(buffer[i+offset]);
		}

		return readCount;
	}

	public void Reset()
	{

	}

	private void Append(float v)
	{
		Buffer[readCursor] = v;
		readCursor++;
		if (readCursor == Buffer.Length)
		{
			SamplesAggregated?.Invoke(this, Buffer);
			readCursor = 0;
		}
	}
}
