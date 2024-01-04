using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicBackend.Model;


internal class PlaylistState
{
	[JsonInclude]
	public string Name { get; set; }
	[JsonInclude]
	public string[] Songs { get; set; }
}

public class Playlist
{
	public List<Song> Songs { get; private set; } = new();
	public string Name { get; internal set; }
	public Playlist(string name)
	{
		Name = name;
	}

	internal PlaylistState DumpState()
	{
		return new()
		{
			Name = Name,
			Songs = Songs.Select(x => x.path).ToArray()
		};
	}

	internal Playlist(PlaylistState playlistState)
	{
		Name = playlistState.Name;
		foreach (var songPath in playlistState.Songs)
		{
			var song = Song.fromPath(songPath);
			if (song is not null)
			{
				Songs.Add(song);
			}
		}
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
