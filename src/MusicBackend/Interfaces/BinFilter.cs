using PlayerApi.Interfaces;
using PlayerApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Interfaces;
public class BinFilter : IFilter
{
	public Channel[] process(Channel[] buffer)
	{
		for (int i = 0; i != buffer.Length; ++i)
		{
			buffer[i] = processBin(buffer[i]);
		}

		return buffer;
	}

	virtual public Channel processBin(Channel bin)
	{
		return bin;
	}
}
