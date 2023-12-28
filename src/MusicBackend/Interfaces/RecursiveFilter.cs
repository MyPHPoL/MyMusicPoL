using PlayerApi.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerApi.Interfaces;

public class RecursiveFilter : IFilter
{
    protected Channel[] previousBuffer { get; private set; }

    public Channel[] process(Channel[] buffer)
    {
        var minLength = Math.Min(buffer.GetLength(1), previousBuffer.GetLength(1));
        for (int i = 0; i != minLength; ++i)
        {
            buffer[i] = processChannels(buffer[i], i);
        }
        return buffer;
    }

    public void refresh(Channel[] newBuffer)
    {
        previousBuffer = newBuffer;
    }

    public virtual Channel processChannels(Channel channels, int index)
    {
        return channels;
    }
}
