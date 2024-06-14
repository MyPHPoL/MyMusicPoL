using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Filters;

public sealed class RectangularWindow : IWindowFunction
{
    private double[] _window;
    public double[] Window
    {
        get => _window;
    }

    public RectangularWindow(int length)
    {
        _window = new double[length];
        for (int i = 0; i != length; ++i)
        {
            _window[i] = 1;
        }
    }
}
