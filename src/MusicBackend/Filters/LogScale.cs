using MusicBackend.Interfaces;
using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;
public class LogScale : IFilter
{
	private int[] logIndices;
	private Channel[] buffer;
	/**
	 * @brief Logarithmic scale filter
	 * @param factor Factor used to calculate how far to jump from one index to the next eg. 1.02 works good
	 * @param sampleRateFactor Sample rate used to calculate current fftBin to end iterating
	 */
	public LogScale(float factor, float sampleRateFactor, int bufferLen)
	{
		List<int> logIndices = new ();
		for (float i = 3; ; i*=factor)
		{
			if (i > sampleRateFactor)
			{
				break;
			}
			logIndices.Add((int)i);
		}

		this.logIndices = logIndices.ToArray();
		buffer = new Channel[logIndices.Count];
	}

	/**
	 * Might throw when buffer doesnt hold enough samples to index them on log scale
	 */
	public Channel[] process(Channel[] buffer)
	{
		for (int i = 0; i != logIndices.Length; ++i)
		{
			this.buffer[i] = buffer[logIndices[i]];
		}
		return this.buffer;
	}
}
