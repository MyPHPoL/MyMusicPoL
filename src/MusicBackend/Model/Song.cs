using System.Runtime.Serialization;

namespace Mp.Model;

public class Song
{
	public string name { get; set; }
	public string path { get; set; }
	[IgnoreDataMember]
	public TimeSpan length { get; set; }

	private Song()
	{
		name = string.Empty;
		path = string.Empty;
		length = TimeSpan.Zero;
	}

	public Song(string path)
	{
		try
		{
			this.path = Path.GetFullPath(path);
			name = Path.GetFileName(path);
			using (var reader = new NAudio.Wave.AudioFileReader(path))
			{
				length = reader.TotalTime;
			}
		} catch
		{
			this.path = string.Empty;
			name = string.Empty;
			length = TimeSpan.Zero;
		}
	}
	static public Song fromPath(string path)
	{
		Song song = new();
		song.path = Path.GetFullPath(path);
		song.name = Path.GetFileName(path);
		using (var reader = new NAudio.Wave.AudioFileReader(path))
		{
			song.length = reader.TotalTime;
		}
		return song;
	}
	public override string ToString()
	{
		return name;
	}
}
