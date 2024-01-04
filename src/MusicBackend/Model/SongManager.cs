﻿namespace MusicBackend.Model;
internal class SongManager
{
	// cache album name -> album
	private readonly Dictionary<string, SongAlbum> albums = new();
	// cache path -> song
	private readonly Dictionary<string, Song> songs = new();

	private static SongManager? instance;
	public static SongManager Instance
	{
		get => instance ??= new SongManager();
	}

	private SongManager()
	{
		albums.Add("Unknown", new SongAlbum() { Cover = null, Name = "Unkown"});
	}

	public Song SongFromPath(string path)
	{
		if (songs.ContainsKey(path))
		{
			return songs[path];
		}
		else
		{
			var tagFile = TagLib.File.Create(path);
			var title = String.IsNullOrWhiteSpace(tagFile.Tag.Title) 
				? Path.GetFileNameWithoutExtension(path)
				: tagFile.Tag.Title;
			Song song = new Song()
			{
				name = title,
				path = System.IO.Path.GetFullPath(path),
				length = tagFile.Properties.Duration,
				artist = tagFile.Tag.FirstPerformer is null ? "Unknown" : tagFile.Tag.FirstPerformer,
				Album = GetAlbum(tagFile)
			};
			songs.Add(path, song);
			return song;
		}
	}

	private SongAlbum GetAlbum(TagLib.File file)
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