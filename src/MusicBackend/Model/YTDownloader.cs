using MusicBackend.Interfaces;
using TagLib;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace MusicBackend.Model;

internal class YTDownloader : IYTDownloader
{
	private readonly YoutubeClient client = new YoutubeClient();
	public async Task<Song> DownloadVideoAsync(string url)
	{
		var video = await this.client.Videos.GetAsync(url).ConfigureAwait(false);
		var title = video.Title;
		var artist = video.Author.ToString();

		string result = Path.GetTempPath();

		// create temp directory
		var outputFilePath = Path.Combine(result, $"mymusicpol");
		Directory.CreateDirectory(outputFilePath);

		// create random file name
		var fileName = (Guid.NewGuid().ToString() + ".mp3");

		var filePath = Path.Combine(outputFilePath, fileName);
		// download song
		await this.client.Videos.DownloadAsync(url, filePath).ConfigureAwait(false);

		AddMetaData(filePath, title, artist);

		return Song.fromPath(filePath);
	}

	private void AddMetaData(string filePath, string title, string artist)
	{
		var file = TagLib.File.Create(filePath);
		file.Tag.Title = title;
		file.Tag.Performers = new string[] { artist };
		file.Save();
	}
}
