using MusicBackend.Interfaces;

namespace MusicBackend.Model;

internal class SongManager
{
	// cache path -> song
	private readonly Dictionary<string, Song> songs = new();
	// yt downloader
	private IYTDownloader downloader = new YTDownloaderCache(new YTDownloader());

	private static SongManager? instance;
	public static SongManager Instance
	{
		get => instance ??= new SongManager();
	}

	private SongManager()
	{
	}

	internal void InitYtCache(YTDownloaderCacheState state)
	{
		downloader = new YTDownloaderCache(new YTDownloader(), state);
	}

	internal YTDownloaderCacheState? DumpYtCache()
	{
		if (downloader is YTDownloaderCache cache)
		{
			return cache.DumpState();
		}
		return null;
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
				Album = SongAlbum.GetAlbum(tagFile)
			};
			songs.Add(path, song);
			return song;
		}
	}

	public Song SongFromUrl(string url)
	{
		var songTask = Task.Run(async () => { return await downloader.DownloadVideoAsync(url).ConfigureAwait(false); });
		songTask.Wait();
		return songTask.Result;
	}

}
