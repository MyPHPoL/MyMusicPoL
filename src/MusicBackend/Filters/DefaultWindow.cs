using MusicBackend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;
public sealed class DefaultWindow : IWindowFunction
{
    private double[] _window;
    public double[] Window { get => _window; }

    public DefaultWindow(int length)
    {
		//_window = new double[length];
        _window = NWaves.Windows.Window.Blackman(length).Select(length => (double)length).ToArray();
	}
}
