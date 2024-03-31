using mymusicpol.Models;
using mymusicpol.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MusicBackend.Model;
using System.Windows.Threading;
using System.Windows.Media;
using Windows.Media;
using System.Windows;
using System.Windows.Automation.Peers;

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

	private string fromWebText; // play from web text input
	public string FromWebText
	{
		get { return fromWebText; }
		set
		{
			fromWebText = value;
			OnPropertyChanged(nameof(FromWebText));
		}
	}

	public Notify<string> TotalTime { get; set; }
	// current song progress
	public Notify<double> ProgressValue { get; set; } 
	// timer for song progress
	private DispatcherTimer timer; 
	// hack to avoid infinite recursion
	private bool isChangingSlider = false;
	// all playlists
	public ObservableCollection<SonglistViewModel> Playlists { get; set; } = new();
	// view model for selected library/playlist/queue
	public SelectedListViewModel SelectedList { get; set; } = new();
	// Index of selected playlist
	public int SelectedIndex { get; set; } = -1;

	public ICommand ShuffleButton { get; }
	public ICommand RepeatButton { get; }
	public ICommand PlayPauseButton { get; }
	public ICommand PreviousButton { get; }
	public ICommand NextButton { get; }
	public ICommand ShowQueueButton { get; }
	public ICommand ShowPlaylistCommand { get; }
	public ICommand ShowLibaryCommand { get; }
	public IAsyncRelayCommand PlayFromWebCommand { get; }
	public Notify<bool> PlayFromWebInProgress { get; set; } = new() { Value = false };
	public Notify<string> PlayPauseLabel { get; set; } = new()
	{
		Value = ""
	};
	public ButtonNotify RepeatLabel { get; set; } = new();
	public ButtonNotify ShuffleLabel { get; set; } = new()
	{
		Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush,
		Content = "\uE8B1"
	};
	// current volume
	public Notify<double> Volume { get; set; } = new(); 
	// volume icon
	public Notify<string> VolumeIcon { get; set; } = new(); 
	
	private readonly WindowsMediaController windowsMediaController;

	class PlaylistObserver : IPlaylistObserver
	{
		private readonly PlayerViewModel playerViewModel;
		public PlaylistObserver(PlayerViewModel playerViewModel)
		{
			this.playerViewModel = playerViewModel;
		}
		public void OnNewPlaylist(string name)
		{
			playerViewModel.Playlists.Add(new(name, []));
		}

		public void OnPlaylistChange(string name)
		{
			playerViewModel.FillPlaylist(name);
		}

		public void OnPlaylistNameEdited(string oldName, string newName)
		{
			playerViewModel.Playlists.First(p => p.Name == oldName).Name = newName;
		}

		public void OnPlaylistRemoved(string name)
		{
			var index = playerViewModel.Playlists.Select((p, i) => (p, i)).First(p => p.p.Name == name).i;
			playerViewModel.Playlists.RemoveAt(index);
		}
	}

	private void FillPlaylists()
	{
		foreach (var playlist in PlaylistManager.Instance.Playlists)
		{
			Playlists.Add(new(playlist.Value.Name, playlist.Value.Songs));
		}
	}
	private void FillPlaylist(string name)
	{
		var playlist = PlaylistManager.Instance.GetPlaylist(name);
		if (playlist is null) return;
		// find index of playlist
		var index = Playlists.Select((p, i) => (p, i)).First(p => p.p.Name == name).i;

		Playlists[index].Name = playlist.Name;
		Playlists[index].Songs.Clear();
		Playlists[index].SetSongs(playlist.Songs);
	}

	public PlayerViewModel()
	{
		// fill commands
		PreviousButton = new RelayCommand(PreviousSongCallback);
		PlayPauseButton = new RelayCommand(PlayPauseSongCallback);
		NextButton = new RelayCommand(NextSongCallback);
		ShuffleButton = new RelayCommand(ShuffleSongCallback);
		RepeatButton = new RelayCommand(RepeatSongCallback);
		ShowPlaylistCommand = new RelayCommand(ShowPlaylist);
		ShowQueueButton = new RelayCommand(ShowQueue);
		ShowLibaryCommand = new RelayCommand(ShowLibaryCallback);
		PlayFromWebCommand = new AsyncRelayCommand<string?>(PlayFromWebCallback);
		windowsMediaController = new(PlayerModel.Instance, QueueModel.Instance);

		PlaylistManager.Instance.Subscribe(new PlaylistObserver(this));
		FillPlaylists();
		//setup data bindings
		setupTimer();
		UpdateVolumeIcon(PlayerModel.Instance.currentVolume());
		ChangeRepeatLabel(QueueModel.Instance.QueueMode);

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
		var curSongPath = PlayerModel.Instance.currentSong();
		if (curSongPath == null)
		{
			CurrentSong = new (null);
		}
		else
		{
			CurrentSong = new (Song.fromPath(curSongPath));
		}

		QueueModel.Instance.OnSongChange += OnSongChange;
		PlayerModel.Instance.OnSongChange += OnSongChange;
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

		QueueModel.Instance.OnQueueModeChange += (repeat) =>
		{
			ChangeRepeatLabel(repeat);
		};
		PlayerModel.Instance.OnTimeChange += (time) =>
		{
			isChangingSlider = true;
			TimeElapsed = formatTime(time);
			ProgressValue.Value = time.TotalSeconds / PlayerModel.Instance.songLength().TotalSeconds;
		};
		Volume.PropertyChanged += (sender, e) =>
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

	private void ShowLibaryCallback()
	{
		SelectedList.ShowLibrary();
	}

	private void ShowQueue()
	{
		SelectedList.ShowQueue();
	}

	private void OnSongChange(Song? song)
	{
		CurrentSong.SetSong(song);
		windowsMediaController.UpdateSong(song);

		TimeElapsed = formatTime(TimeSpan.Zero);
		ProgressValue.Value = 0.0;
		if (song is null)
		{
			TotalTime.Value = formatTime(TimeSpan.Zero);
			pauseTimer();
		}
		else
		{
			TotalTime.Value = formatTime(song.length);
			startTimer();
		}
	}
  
	private void ChangeRepeatLabel(QueueMode queueMode)
	{
		// seperate enum into two different cases
		var (repeat,random) = queueMode switch
		{
			QueueMode.Loop => (1,false),
			QueueMode.OneLoop => (0,false),
			QueueMode.RandomLoop => (1,true),
			QueueMode.Single => (2,false),
			_ => (0,false),
		};

		if (repeat == 0)
		{
			RepeatLabel.Background = new BrushConverter().ConvertFrom("#d2b4de") as Brush;
			RepeatLabel.Content = "\uE8ED";
		}
		else if (repeat == 1)
		{
			RepeatLabel.Content = "\uE8EE";
			RepeatLabel.Background = new BrushConverter().ConvertFrom("#d2b4de") as Brush;
		}
		else if (repeat == 2)
		{
			RepeatLabel.Content = "\uE8EE";
			RepeatLabel.Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush;
		}
		if (random)
		{
			ShuffleLabel.Background = new BrushConverter().ConvertFrom("#d2b4de") as Brush;
		}
		else
		{
			ShuffleLabel.Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush;
		}
	}
	private void UpdateVolumeIcon(float value)
	{
		VolumeIcon.Value = value switch
		{
			<= 0 => "\ue74f",
			>= 0.00F and < 0.33F => "\ue993",
			>= 0.33F and < 0.66F => "\ue994",
			>= 0.66F => "\ue995",
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
		var mode = QueueModel.Instance.QueueMode;
		var nextMode = mode switch
		{
			QueueMode.Loop => QueueMode.OneLoop,
			QueueMode.OneLoop => QueueMode.Single,
			QueueMode.Single => QueueMode.Loop,
			_ => QueueMode.Loop,
		};

		QueueModel.Instance.SetQueueMode(nextMode);
	}
	private void ShuffleSongCallback()
	{
		var mode = QueueModel.Instance.QueueMode;
		var nextMode = mode switch
		{
			QueueMode.RandomLoop => QueueMode.Loop,
			_ => QueueMode.RandomLoop,
		};
		QueueModel.Instance.SetQueueMode(nextMode);
	}
	public bool NewPlaylist(string name)
	{
		return PlaylistManager.Instance.CreatePlaylist(name);
	}

	public void EditPlaylist(int index, string newName)
	{
		if (index < 0 || index >= Playlists.Count) return;
		var oldName = Playlists[index].Name;
		PlaylistManager.Instance.EditPlaylistName(oldName,newName);
	}
	public void ShowPlaylist()
	{
		if (SelectedIndex < 0 || SelectedIndex >= Playlists.Count) return;
		SelectedList.ShowPlaylist(Playlists[SelectedIndex].Name);
	}
	public void DeletePlaylist(int index)
	{
		if (index < 0 || index >= Playlists.Count) return;
		PlaylistManager.Instance.RemovePlaylist(Playlists[index].Name);
	}
	public void PlayPlaylist(int index)
	{
		if (index < 0 || index >= Playlists.Count) return;
		QueueModel.Instance.PlayPlaylist(Playlists[index].Name);
		SelectedList.ShowQueue();
	}
	public void SelectedListRemove(int index)
	{
		SelectedList.RemoveSong(index);
	}
	public void SelectedListAddQueue(int index)
	{
		SelectedList.AddToQueue(index);
	}
	public void SelectedListPlay(int index)
	{
		SelectedList.PlayNth(index);
	}
	public void SelectedListAddPlaylist(int index, string playlistName)
	{
		SelectedList.AddToPlaylist(playlistName,index);
	}
	public void SelectedListImport(string filename)
	{
		SelectedList.ImportPlaylist(filename);
	}
	public void SelectedListExport(string filename)
	{
		SelectedList.ExportPlaylist(filename);
	}
	private async Task PlayFromWebCallback(string? text)
	{
		if (String.IsNullOrWhiteSpace(text)) return;
		PlayFromWebInProgress.Value = true;
		var song = await Song.fromUrlAsync(text);
		if (song is not null)
		{
			QueueModel.Instance.appendSong(song);
			MessageBox.Show("Song added to queue");
		}
		PlayFromWebInProgress.Value = false;
	}
}
