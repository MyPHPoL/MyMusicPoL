using MusicBackend.Interfaces;
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

		AddTitle(filePath, title);

		return Song.fromPath(filePath);
	}

	private void AddTitle(string filePath, string title)
	{
		var file = TagLib.File.Create(filePath);
		file.Tag.Title = title;
		file.Save();
	}
}
