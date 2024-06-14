using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Filters;

public sealed class LogScale : IFilter
{
    private int[] logIndices;
    private double[] buffer;

    /**
     * @brief Logarithmic scale filter
     * @param factor Factor used to calculate how far to jump from one index to the next eg. 1.02 works good
     * @param sampleRateFactor Sample rate used to calculate current fftBin to end iterating
     */
    public LogScale(double factor, int bufferLen)
    {
        List<int> logIndices = new();
        int prev = 0;
        for (double i = 3; i < bufferLen; i *= factor)
        {
            if (prev != (int)i)
            {
                logIndices.Add((int)i);
            }
            prev = (int)i;
        }

        this.logIndices = logIndices.ToArray();
        buffer = new double[logIndices.Count];
    }

    /**
     * Might throw when buffer doesnt hold enough samples to index them on log scale
     */
    public double[] process(double[] buffer)
    {
        for (int i = 0; i != logIndices.Length; ++i)
        {
            this.buffer[i] = buffer[logIndices[i]];
        }
        return this.buffer;
    }
}
