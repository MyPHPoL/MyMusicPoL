using MusicBackend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;
public class LerpFilter : IFilter
{
	float increment;
	int factor;
	double[] buffer;
	public LerpFilter(int factor, int bufferLength)
	{
		increment = 1.0f / factor;
		this.factor = factor;
		buffer = new double[bufferLength];
	}

	private double Lerp(double from, double to, double t)
	{
		return from + (to - from) * t;
	}
	public double[] process(double[] buffer)
	{
		if (buffer.Length != this.buffer.Length*factor)
		{
			this.buffer = new double[buffer.Length*factor];
		}
		for (int i = 0; i != buffer.Length-1; ++i)
		{
			for (int j = 0; j != factor; ++j)
			{
				this.buffer[i * factor + j] = Lerp(buffer[i], buffer[i + 1], j * increment);
			}
		}
		return this.buffer;
	}
}
