using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Model;

namespace MusicBackend.Interfaces;

public interface IWindowFunction
{
    public double[] Window { get; }
}

public class WindowFunction : IFilter
{
    private IWindowFunction windowFunction;

    public WindowFunction(IWindowFunction windowFunction)
    {
        this.windowFunction = windowFunction;
    }

    public double[] process(double[] buffer)
    {
        var window = windowFunction.Window;
        int minLength = Math.Min(window.Length, buffer.Length);
        for (int i = 0; i != minLength; ++i)
        {
            buffer[i] *= window[i];
        }
        return buffer;
    }
}
