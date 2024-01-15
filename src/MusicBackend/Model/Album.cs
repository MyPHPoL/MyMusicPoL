using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Model;

public class SongAlbum
{
	private static readonly Dictionary<string, SongAlbum> albums = new()
	{
		{ "Unknown", new SongAlbum() { Cover = null, Name = "Unknown" } }
	};
	public string Name { get; init; }
	public byte[]? Cover { get; init; }

	private SongAlbum() { }
	public static SongAlbum GetAlbum(TagLib.File file)
	{
		var albumName = file.Tag.Album is null ? "Unknown" : file.Tag.Album;

		if (albums.ContainsKey(albumName))
		{
			return albums[albumName];
		}
		else
		{
			var album = new SongAlbum()
			{
				Name = albumName,
				Cover = file.Tag.Pictures.Length > 0 ? file.Tag.Pictures[0].Data.Data : null
			};
			albums.Add(albumName, album);
			return album;
		}
	}

}
