using mymusicpol.Models;
using mymusicpol.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MusicBackend.Model;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace mymusicpol.ViewModels;

public struct VolumeControl
{
	public Notify<double> SliderValue { get; set; }
	public Notify<string> VolumeValue { get; set; }
}

public struct TimeControl
{
	public Notify<double> SliderValue { get; set; }
	public Notify<string> TimeValue { get; set; }
}

internal class SongViewModel : INotifyPropertyChanged
{
	private string _title { get; set; }
	private string _artist { get; set; }
	private string _album { get; set; } 
	private BitmapSource _cover { get; set; }

	public SongViewModel(MusicBackend.Model.Song? song)
	{
		SetSong(song);
	}

	public void SetSong(MusicBackend.Model.Song? song)
	{
		if (song is not null)
		{
			this.title = song.name;
			this.artist = song.artist;
			this.album = song.Album.Name;
		}
		else
		{
			this.title = "No song selected";
			this.artist = "";
			this.album = "";
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


//this is a test (i copied youtube tutorial video like a monkey)
internal class PlayerViewModel : ViewModelBase
{
	// current song model
	public SongViewModel CurrentSong { get; set; }

	private string timeElapsed; // current song time elapsed
	public string TimeElapsed
	{
		get { return timeElapsed; }
		set
		{
			timeElapsed = value;
			OnPropertyChanged(nameof(TimeElapsed));
		}
	}

	public Notify<string> TotalTime { get; set; }

	public Notify<double> ProgressValue { get; set; } // current song progress

	private string selectedListName; // current selected playlist name
	public string SelectedListName
	{
		get { return selectedListName; }
		set
		{
			selectedListName = value;
			OnPropertyChanged(nameof(SelectedListName));
		}
	}
	// timer for song progress
	private DispatcherTimer timer; 
	// hack to avoid infinite recursion
	public bool isChangingSlider = false;
	
	private readonly ObservableCollection<SonglistViewModel> playlists; // all playlists
	public ObservableCollection<SongViewModel> SelectedList { get; set; } = new();
	public IEnumerable<SonglistViewModel> Playlists => playlists;
	public ICommand Show_Library { get; }
	public ICommand ShuffleButton { get; }
	public ICommand RepeatButton { get; }
	public ICommand PlayPauseButton { get; }
	public ICommand PreviousButton { get; }
	public ICommand NextButton { get; }
	public Notify<string> PlayPauseLabel { get; set; } = new()
	{
		Value = ""
	};
	public ButtonNotify RepeatLabel	{ get; set; } = new()
	{
		Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush,
		Content = ""
	};
	public ButtonNotify ShuffleLabel { get; set; } = new()
	{
		Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush,
		Content = "\uE8B1"
	};
	public Notify<double> Volume { get; set; } = new(); // current volume
	public Notify<string> VolumeIcon { get; set; } = new(); // volume icon

	public PlayerViewModel()
	{
		playlists =
		[
			new SonglistViewModel(new Songlist("Playlist 1", new())),
			new SonglistViewModel(new Songlist("Playlist 2 Different Name", new())),
			new SonglistViewModel(new Songlist("Playlist 3 LOLOLO", new())),
			new SonglistViewModel(new Songlist("Playlist 4 xdd", new())),
			new SonglistViewModel(new Songlist("Playlist 5 test", new())),
			new SonglistViewModel(new Songlist("Playlist 6 oho", new())),
			new SonglistViewModel(new Songlist("Playlist 7 haahahahahah", new())),
		];

		FillSelectedList();

		// fill commands
		PreviousButton = new RelayCommand(PreviousSongCallback);
		PlayPauseButton = new RelayCommand(PlayPauseSongCallback);
		NextButton = new RelayCommand(NextSongCallback);
		ShuffleButton = new RelayCommand(ShuffleSongCallback);
		RepeatButton = new RelayCommand(RepeatSongCallback);

		//setup data bindings
		setupTimer();
		UpdateVolumeIcon(PlayerModel.Instance.currentVolume());

		Volume.Value = PlayerModel.Instance.currentVolume() * 100;
		ProgressValue = new()
		{
			Value = PlayerModel.Instance.currentTime().TotalSeconds / PlayerModel.Instance.songLength().TotalSeconds
		};
		TotalTime = new()
		{
			Value = formatTime(PlayerModel.Instance.songLength())
		};
		timeElapsed = formatTime(PlayerModel.Instance.currentTime());
		var curSong = QueueModel.Instance.currentSong();
		CurrentSong = new (curSong);
		selectedListName = "Test Playlist Name";
		QueueModel.Instance.OnSongChange += (song) =>
		{
			CurrentSong.SetSong(song);
			TotalTime.Value = formatTime(song.length);
			timeElapsed = formatTime(TimeSpan.Zero);

			startTimer();
			PlayerModel.Instance.selectSong(song.path);
		};
		PlayerModel.Instance.OnVolumeChange += (vol) =>
		{
			UpdateVolumeIcon(vol);
		};
		PlayerModel.Instance.OnPlaybackChange += (state) =>
		{
			if (state.isPlaying())
			{
				PlayPauseLabel.Value = "";
			}
			else
			{
				PlayPauseLabel.Value = "";
			}
		};

		QueueModel.Instance.OnQueueModified += () =>
		{
			SelectedList.Clear();
			FillSelectedList();
		};

		QueueModel.Instance.OnRepeatChange += (repeat) =>
		{
			if (repeat)
			{
				RepeatLabel.Content = "\uE8EE";
				RepeatLabel.Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush;
			}
			else
			{
				RepeatLabel.Background = new BrushConverter().ConvertFrom("#d2b4de") as Brush;
				RepeatLabel.Content = "\uE8ED";
			}
		};

		PlayerModel.Instance.OnTimeChange += (time) =>
		{
			isChangingSlider = true;
			TimeElapsed = formatTime(time);
			ProgressValue.Value = time.TotalSeconds / PlayerModel.Instance.songLength().TotalSeconds;
		};
		PlayerModel.Instance.attachOnSongEnd(() =>
		{
			QueueModel.Instance.nextSong();
		});
		VolumeIcon.PropertyChanged += (sender, e) =>
		{
			PlayerModel.Instance.setVolume((float)Volume.Value / 100);
		};
		ProgressValue.PropertyChanged += (sender, e) =>
		{
			// avoid infinite recursion
			if (isChangingSlider)
			{
				isChangingSlider = false;
				return;
			}
			OnSliderValueChanged(ProgressValue.Value);
		};

	}

	private void UpdateVolumeIcon(float value)
	{
		if (value == 0)
		{
			VolumeIcon.Value = "";
		}
		else
		{
			VolumeIcon.Value = "";
		}
	}

	private void FillSelectedList()
	{
		foreach (var song in QueueModel.Instance.songs)
		{
			SelectedList.Add(new(song));
		}
	}

	public void OnSliderValueChanged(double value)
	{
		var playerModel = PlayerModel.Instance;
		var currentTime = value * playerModel.songLength().TotalSeconds;
		playerModel.setTime(TimeSpan.FromSeconds(currentTime));
	}
	private void PreviousSongCallback()
	{
		QueueModel.Instance.forcePrevSong();
	}
	private void NextSongCallback()
	{
		QueueModel.Instance.forceNextSong();
	}

	private static string formatTime(TimeSpan time)
	{
		return time.ToString("h\\:mm\\:ss");
	}
	private void PlayPauseSongCallback()
	{
		var playerModel = PlayerModel.Instance;
		if (playerModel.playbackState().isPlaying())
		{
			pauseTimer();
			playerModel.pause();
		}
		else
		{
			startTimer();
			playerModel.play();
		}
	}
	private void setupTimer()
	{
		timer = new();
		timer.Interval = TimeSpan.FromSeconds(1);
		timer.Tick += OnTimerCallback;
	}
	private void startTimer() { timer.Start(); }
	private void pauseTimer()
	{
		timer.Stop();
		var remainingTime = PlayerModel.Instance.currentTime();
		if (remainingTime.Microseconds > 0)
		{
			timer.Interval = remainingTime;
			timer.Tick += (s, elapsed) =>
			{
				fixupTimer();
			};
		}
		else
		{
			fixupTimer();
		}
	}
	private void fixupTimer()
	{
		PlayerModel.Instance.UpdateTime();
		timer.Interval = TimeSpan.FromSeconds(1);
		timer.Tick += OnTimerCallback;
	}
	private void OnTimerCallback(object? sender, EventArgs e)
	{
		isChangingSlider = true;
		PlayerModel.Instance.UpdateTime();
	}
	private void setupTimers()
	{
		timer = new();
		timer.Interval = TimeSpan.FromSeconds(1);
		timer.Tick += OnTimerCallback;
	}
	private void RepeatSongCallback()
	{
		QueueModel.Instance.toggleRepeat();
	}
	private void ShuffleSongCallback()
	{
		QueueModel.Instance.shuffleQueue();

	}
}
