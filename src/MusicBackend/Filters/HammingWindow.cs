using MusicBackend.Interfaces;
using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters
{
    public sealed class HammingWindow : IWindowFunction
    {
        private Channel[] _window;
        public Channel[] Window { get => _window; }
        public HammingWindow(int length)
        {
            _window = new Channel[length];
            for(int i = 0; i != length; ++i)
            {
                float val = 0.53836f - 0.46164f * (float)Math.Cos(Math.PI * 2 * i / length);
                _window[i] = new() { left = val, right = val };
            }
        }
    }
}
