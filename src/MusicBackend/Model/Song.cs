using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MusicBackend.Model;

public class Song
{
	public string path { get; set; }
	[JsonIgnore]
	public string name { get; set; }
	[JsonIgnore]
	public string artist { get; set; }
	[JsonIgnore]
	public TimeSpan length { get; set; }
	[JsonIgnore]
	public SongAlbum Album { get; set; }

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
			//this.path = Path.GetFullPath(path);
			//name = Path.GetFileName(path);
			//using (var reader = new NAudio.Wave.AudioFileReader(path))
			//{
			//	length = reader.TotalTime;
			//}
		} catch
		{
			this.path = string.Empty;
			this.name = string.Empty;
			this.length = TimeSpan.Zero;
		}
	}
	static public Song fromPath(string path)
	{
		var song = SongManager.Instance.SongFromPath(path);
		return song;
	}
	public override string ToString()
	{
		return name;
	}
}
