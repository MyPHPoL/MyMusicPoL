using MusicBackend.Interfaces;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;

namespace MusicBackend.Model;

internal class YTDownloader : IYTDownloader
{
	private readonly YoutubeClient client = new YoutubeClient();
	public async Task<Song> DownloadVideoAsync(string url, Action<double> progressFunc)
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

		var progressIndicator = new Progress<double>(progressFunc);
		await this.client.Videos.DownloadAsync(url, filePath,progressIndicator).ConfigureAwait(false);


		var imageUrl = video.Thumbnails.FirstOrDefault()?.Url;

		var (imageData,mimeType) = imageUrl is not null
			? await FetchImage(imageUrl).ConfigureAwait(false)
			: ([],null);


		AddMetaData(filePath, title, artist, imageData,mimeType);

		var song = Song.fromPath(filePath);
		if (song is null) throw new Exception("Failed to create song from path");
		return song;
	}

	private async Task<(byte[] data,string? mime)> FetchImage(string url)
	{
		using var client = new HttpClient();
		var response = await client.GetAsync(url).ConfigureAwait(false);
		var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
		return (data, response.Content.Headers.ContentType?.MediaType);
	}

	private void AddMetaData(string filePath, string title, string artist, byte[] data, string? mimeType)
	{
		var file = TagLib.File.Create(filePath);
		file.Tag.Title = title;
		file.Tag.Performers = [artist];
		if (data.Length != 0 || mimeType is not null)
		{
			var picture = new TagLib.Picture
			{
				Type = TagLib.PictureType.FrontCover,
				Data = new TagLib.ByteVector(data),
				MimeType = mimeType
			};
			file.Tag.Pictures = [picture];
			file.Tag.Album = title + " " + artist;
		}
		file.Save();
	}
}
