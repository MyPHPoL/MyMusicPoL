using MusicBackend.Interfaces;
using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;

public sealed class HammingWindow : IWindowFunction
{
    private double[] _window;
    public double[] Window { get => _window; }
    public HammingWindow(int length)
    {
        _window = new double[length];
        for(int i = 0; i != length; ++i)
        {
            double val = 0.53836f - 0.46164f * Math.Cos(Math.PI * 2 * i / length);
            _window[i] = val;
        }
    }
}
