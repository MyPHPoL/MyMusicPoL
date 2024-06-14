using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Interfaces;

public class BinFilter : IFilter
{
    public double[] process(double[] buffer)
    {
        for (int i = 0; i != buffer.Length; ++i)
        {
            buffer[i] = processBin(buffer[i]);
        }

        return buffer;
    }

    public virtual double processBin(double bin)
    {
        return bin;
    }
}
