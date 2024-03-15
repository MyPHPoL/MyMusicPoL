using MusicBackend.Interfaces;
using System.Text.Json.Serialization;

namespace MusicBackend.Model;



internal class YTDownloaderCacheState
{
    [JsonInclude]
    public UrlSongPath[]? cache;

    public class UrlSongPath
    {
        [JsonInclude]
		public string url { get; set; }
        [JsonInclude]
		public string songPath { get; set; }
	}
}

internal class YTDownloaderCache : YTDownloader, IYTDownloader
{
	private readonly IYTDownloader _downloader;
	private readonly Dictionary<string, Song> _cache;

	public YTDownloaderCache(IYTDownloader downloader)
	{
		this._downloader = downloader;
		this._cache = new();
	}

	internal YTDownloaderCache(IYTDownloader downloader, YTDownloaderCacheState state)
	{
		this._downloader = downloader;
		this._cache = new();
		if (state.cache is null)
		{
			return;
		}
		foreach (var cache in state.cache)
		{
			var song = Song.fromPath(cache.songPath);
			if (song is null) continue;
			this._cache.Add(cache.url, song);
		}
	}

	internal YTDownloaderCacheState DumpState()
	{
		var state = new YTDownloaderCacheState()
		{
			cache = this._cache.Select(
				x => new YTDownloaderCacheState.UrlSongPath()
				{
					url = x.Key,
					songPath = x.Value.path
				}).ToArray()
		};
		return state;
	}

	public async Task<Song> DownloadVideoAsync(string url)
	{
		if (this._cache.ContainsKey(url) && Path.Exists(this._cache[url].path))
		{
			return this._cache[url];
		}
		else
		{
			var song = await this._downloader.DownloadVideoAsync(url).ConfigureAwait(false);
			if (this._cache.ContainsKey(url))
			{
				this._cache[url] = song;
			}
			else
				this._cache.Add(url, song);
			return song;
		}
	}
}