using mymusicpol.Models;
using mymusicpol.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Mp.Model;
using System.Windows.Threading;
using System.ComponentModel;

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
	private string _title { get; set; } = "Unknown Title";
	private string _artist { get; set; } = "Unknown Artist";
	private string _album { get; set; } = "Unknown Album";
	private string _path { get; set; }
	private string? _cover { get; set; }

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
	public string? cover
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
	private string cover; // current song cover
	public string Cover
	{
		get { return cover; }
		set
		{
			cover = value;
			OnPropertyChanged(nameof(Cover));
		}
	}

	private string title; // current song title
	public string Title
	{
		get { return title; }
		set
		{
			title = value;
			OnPropertyChanged(nameof(Title));
		}
	}

	private string artist; // current song artist
	public string Artist
	{
		get { return artist; }
		set
		{
			artist = value;
			OnPropertyChanged(nameof(Artist));
		}
	}

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
	public Notify<double> Volume { get; set; } // current volume

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

		foreach(var song in QueueModel.Instance.songs)
		{
			SelectedList.Add(new()
			{
				cover = null,
				path = song.path,
				title = song.name
			});
		}

		// fill commands
		PreviousButton = new RelayCommand(PreviousSongCallback);
		PlayPauseButton = new RelayCommand(PlayPauseSongCallback);
		NextButton = new RelayCommand(NextSongCallback);
		ShuffleButton = new RelayCommand(ShuffleSongCallback);
		RepeatButton = new RelayCommand(RepeatSongCallback);

		//setup data bindings
		setupTimer();
		Volume = new()
		{
			Value = (int)(PlayerModel.Instance.currentVolume()*100)
		};
		ProgressValue = new()
		{
			Value = PlayerModel.Instance.currentTime().TotalSeconds / PlayerModel.Instance.songLength().TotalSeconds
		};
		TotalTime = new()
		{
			Value = formatTime(PlayerModel.Instance.songLength())
		};
		timeElapsed = formatTime(PlayerModel.Instance.currentTime());
		// IDK HOW TO SHOW THIS DAMN COVER
		cover = "assets\\TEST-BOX-100px-100px.png";
		title = QueueModel.Instance.currentSong()?.name ?? "No song selected";
		artist = "Test Artist";
		selectedListName = "Test Playlist Name";
		QueueModel.Instance.OnSongChange += (song) =>
		{
			title = song.name;
			TotalTime.Value = formatTime(song.length);
			timeElapsed = formatTime(TimeSpan.Zero);
			
			startTimer();
			PlayerModel.Instance.selectSong(song.path);
		};
		//PlayerModel.Instance.OnVolumeChange += (vol) =>
		//{
		//	Volume.Value = vol*100;
		//};

		QueueModel.Instance.OnQueueModified += () =>
		{
			SelectedList.Clear();
			foreach (var song in QueueModel.Instance.songs)
			{
				SelectedList.Add(new SongViewModel()
				{
					title = song.name,
					path = song.path,
					cover = null
				});
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
		Volume.PropertyChanged += (sender, e) =>
		{
			PlayerModel.Instance.setVolume((float)Volume.Value/100);
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
		//SongList.Clear();
		//var songList = QueueModel.Instance.songs;
		//foreach (var song in songList)
		//{
		//	SongList.Add(new SongEntry() { Name = song.name });
		//}
	}
}
