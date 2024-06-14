using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Model;

namespace MusicBackend.Interfaces;

public class RecursiveFilter : IFilter
{
    protected double[] targetBuffer { get; private set; }

    public double[] process(double[] buffer)
    {
        var minLength = Math.Min(buffer.Length, targetBuffer.Length);
        for (int i = 0; i != minLength; ++i)
        {
            buffer[i] = processBin(buffer[i], i);
        }
        return buffer;
    }

    public void refresh(double[] newBuffer)
    {
        targetBuffer = newBuffer;
    }

    public virtual double processBin(double channels, int index)
    {
        return channels;
    }
}
