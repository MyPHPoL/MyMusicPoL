using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MusicBackend.Model;

public class Song
{
	public string path { get; internal set; }
	[JsonIgnore]
	public string name { get; internal set; }
	[JsonIgnore]
	public string artist { get; internal set; }
	[JsonIgnore]
	public TimeSpan length { get; internal set; }
	[JsonIgnore]
	public SongAlbum Album { get; internal set; }

	internal Song()
	{
		name = string.Empty;
		path = string.Empty;
		length = TimeSpan.Zero;
	}

	public Song(string path)
	{
		try
		{
			var song = SongManager.Instance.SongFromPath(path);
			this.path = song.path;
			this.name = song.name;
			this.length = song.length;
			this.Album = song.Album;
			this.artist = song.artist;
		} catch
		{
			this.path = string.Empty;
			this.name = string.Empty;
			this.length = TimeSpan.Zero;
		}
	}
	static public Song? fromPath(string path)
	{
		try
		{
			var song = SongManager.Instance.SongFromPath(path);
			return song;
		}
		catch
		{
			return null;
		}
	}
	public override string ToString()
	{
		return name;
	}
}
