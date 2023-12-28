using MusicBackend.Interfaces;
using PlayerApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Filters;
public sealed class LogBinFilter : BinFilter
{
	private float scale1, scale2;
	public LogBinFilter(float scale1, float scale2)
	{
		this.scale1 = scale1;
		this.scale2 = scale2;
	}
	public override Channel processBin(Channel bin)
	{
		float leftBin = scale1*MathF.Log(scale2*(bin.left));
		float rightBin = scale1*MathF.Log(scale2*(bin.right));
		return new()
		{
			left = leftBin > 0 ? leftBin : 0,
			right = rightBin > 0 ? rightBin : 0
		};
	}
}
