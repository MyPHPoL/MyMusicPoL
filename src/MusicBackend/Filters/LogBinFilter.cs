using MusicBackend.Interfaces;
using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;
public sealed class LogBinFilter : BinFilter
{
	private double scale1, scale2;
	public LogBinFilter(double scale1, double scale2)
	{
		this.scale1 = scale1;
		this.scale2 = scale2;
	}
	public override double processBin(double bin)
	{
		double newBin = scale1*Math.Log(scale2*(bin)+1);
		return newBin > 0 ? newBin : 0;
	}
}
