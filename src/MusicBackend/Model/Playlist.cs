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

	public static Playlist? Import(string path)
	{
		var importer = new PlaylistImporter(path);
		var playlist = importer.Import();
		return playlist;
	}

	public void Export(string path)
	{
		var exporter = new PlaylistImporter(path);
		exporter.Export(this);
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

	public int MoveSongUp(int index)
	{
		if (index < 0 || index >= Songs.Count)
		{
			return -1;
		}
		var song = Songs[index];
		Songs.RemoveAt(index);
		if (index - 1 == -1)
		{
			Songs.Add(song);
			return Songs.Count - 1;
		}
		else
		{
			Songs.Insert(index - 1, song);
			return index - 1;
		}
	}

	public int MoveSongDown(int index)
	{
		if (index < 0 || index >= Songs.Count)
		{
			return -1;
		}
		var song = Songs[index];
		Songs.RemoveAt(index);
		if (index + 1 > Songs.Count)
		{
			Songs.Insert(0,song);
			return 0;
		}
		else
		{
			Songs.Insert(index+1, song);
			return index + 1;
		}
	}
}
