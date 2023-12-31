using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Interfaces;

public class RecursiveFilter : IFilter
{
    protected double[] previousBuffer { get; private set; }

    public double[] process(double[] buffer)
    {
        var minLength = Math.Min(buffer.Length, previousBuffer.Length);
        for (int i = 0; i != minLength; ++i)
        {
            buffer[i] = processBin(buffer[i], i);
        }
        return buffer;
    }

    public void refresh(double[] newBuffer)
    {
        previousBuffer = newBuffer;
    }

    public virtual double processBin(double channels, int index)
    {
        return channels;
    }
}
