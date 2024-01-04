using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace mymusicpol.Models;
public class AlbumCoverManager
{
	private Dictionary<string, BitmapSource> covers = new();

	private static AlbumCoverManager? instance;
	public static AlbumCoverManager Instance { get => instance ??= new AlbumCoverManager(); }
	private AlbumCoverManager()
	{
	}

	private void CreateDefault()
	{
		var image = new BitmapImage();
		image.BeginInit();
		image.UriSource = new Uri("pack://application:,,,/assets/TEST-BOX-100px-100px.png");
		image.EndInit();
		image.Freeze();
		covers["Unknown"] = image;
	}

	public BitmapSource GetCover(MusicBackend.Model.Song? song)
	{
		if (song is null)
		{
			if (covers.ContainsKey("Unknown"))
			{
				return covers["Unknown"];
			}
			CreateDefault();
			return covers["Unknown"];
		}
		if (covers.TryGetValue(song.Album.Name, out BitmapSource? value))
		{
			return value;
		}
		//if (covers.ContainsKey(song.Album.Name))
		//{
		//	return covers[song.Album.Name];
		//}
		var cover = song.Album.Cover;
		var image = new BitmapImage();

		if (cover is not null)
		{
			using var ms = new System.IO.MemoryStream(cover);
			ms.Position = 0;
			image.BeginInit();
			image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = null;
			image.StreamSource = ms;
			image.EndInit();
		}
		else
		{
			image.BeginInit();
			image.UriSource = new Uri("pack://application:,,,/assets/TEST-BOX-100px-100px.png");
			image.EndInit();
		}
		image.Freeze();
		covers[song.Album.Name] = image;

		return image;
	}
}
