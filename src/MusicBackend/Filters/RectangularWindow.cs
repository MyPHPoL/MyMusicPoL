using MusicBackend.Interfaces;
using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;

public sealed class RectangularWindow : IWindowFunction
{
    private Channel[] _window;
    public Channel[] Window { get => _window; }
    public RectangularWindow(int length)
    {
        _window = new Channel[length];
        for(int i = 0; i != length; ++i)
        {
            _window[i] = new() { left = 1, right = 1 };
        }
    }
}
