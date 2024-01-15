using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Model;

public interface ISongAlbum
{
	string Name { get; }
	byte[]? Cover { get; }
}
