using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Interfaces;
using MusicBackend.Model;

namespace MusicBackend.Filters;

public sealed class SmoothingFilter : RecursiveFilter
{
    private double dampingFactor;

    public SmoothingFilter(double dampingFactor)
    {
        this.dampingFactor = dampingFactor;
    }

    public override double processBin(double channels, int index)
    {
        var prevChannel = targetBuffer[index];
        var delta = prevChannel - channels;
        return channels + delta * dampingFactor;
    }
}
