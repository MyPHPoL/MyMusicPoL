using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBackend.Interfaces;

namespace MusicBackend.Filters;

public sealed class ClampBinFilter : BinFilter
{
    private double threshold;

    public ClampBinFilter(double threshold)
    {
        this.threshold = threshold;
    }

    public override double processBin(double bin)
    {
        return bin > threshold ? threshold : bin;
    }
}
