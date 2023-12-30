using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Interfaces;

public interface IWindowFunction
{
    public Channel[] Window { get; }
}
public class WindowFunction : IFilter
{
    private IWindowFunction windowFunction;
    public WindowFunction(IWindowFunction windowFunction)
    {
        this.windowFunction = windowFunction;
    }
    public Channel[] process(Channel[] buffer)
    {
        var window = windowFunction.Window;
        int minLength = Math.Min(window.Length, buffer.Length);
        for (int i = 0; i != minLength; ++i)
        {
            buffer[i].left *= window[i].left;
            buffer[i].right *= window[i].right;
        }
        return buffer;
    }
}
