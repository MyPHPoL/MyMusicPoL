using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Model;
public class Playlist
{
	public List<Song> Songs { get; private set; } = new();
	public string Name { get; internal set; }
	public Playlist(string name)
	{
		Name = name;
	}

	public bool Add(string path)
	{
		var song = Song.fromPath(path);
		if (song is null)
		{
			return false;
		}
		Songs.Add(song);
		return true;
	}

	public bool Remove(string path)
	{
		var status = Songs.RemoveAll(x => x.path == path);
		return status > 0;
	}
	public bool RemoveAt(int index)
	{
		if (index < 0 || index >= Songs.Count)
		{
			return false;
		}
		Songs.RemoveAt(index);
		return true;
	}
}
