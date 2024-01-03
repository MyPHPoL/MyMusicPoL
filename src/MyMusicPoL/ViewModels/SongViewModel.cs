using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace mymusicpol.ViewModels;
internal class SongViewModel : INotifyPropertyChanged
{
	private string _title { get; set; }
	private string _artist { get; set; }
	private string _album { get; set; } 
	private BitmapSource _cover { get; set; }
	private string _path { get; set; }

	public SongViewModel(MusicBackend.Model.Song? song)
	{
		SetSong(song);
	}

	// TODO: Implement caching of images with albums
	public void SetSong(MusicBackend.Model.Song? song)
	{
		if (song is not null)
		{
			this.title = song.name;
			this.artist = song.artist;
			this.album = song.Album.Name;
			this.path = song.path;
		}
		else
		{
			this.title = "No song selected";
			this.artist = "";
			this.album = "";
			this.path = "";
		}
		var cover = song?.Album.Cover;
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
		this.cover = image;
	}

	public string title
	{
		get => _title;
		set
		{
			_title = value;
			OnPropertyChanged(nameof(title));
		}
	}
	public string artist
	{
		get => _artist;
		set
		{
			_artist = value;
			OnPropertyChanged(nameof(artist));
		}
	}
	public string album
	{
		get => _album;
		set
		{
			_album = value;
			OnPropertyChanged(nameof(album));
		}
	}
	public string path
	{
		get => _path;
		set
		{
			_path = value;
			OnPropertyChanged(nameof(path));
		}
	}

	public BitmapSource cover
	{
		get => _cover;
		set
		{
			_cover = value;
			OnPropertyChanged(nameof(cover));
		}
	}
	#region INotifyPropertyChanged Members
	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}
