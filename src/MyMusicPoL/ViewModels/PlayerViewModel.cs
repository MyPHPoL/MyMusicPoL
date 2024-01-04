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
	public ICommand NewPlaylistButton { get; }
	public ICommand ShowQueueButton { get; }
	public ICommand ShowPlaylistCommand { get; }
	public ICommand ShowLibaryCommand { get; }
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
			//if (playerViewModel.SelectedList.Name == name)
			//{
			//	playerViewModel.SelectedList.ClearAll();
			//}
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
		NewPlaylistButton = new RelayCommand(NewPlaylistCallback);
		ShowPlaylistCommand = new RelayCommand(ShowPlaylist);
		ShowQueueButton = new RelayCommand(ShowQueue);
		ShowLibaryCommand = new RelayCommand(ShowLibaryCallback);
		//PlaylistEditedButton = new RelayCommand<string>(EditedPlaylistCallback);

		PlaylistManager.Instance.Subscribe(new PlaylistObserver(this));
		//setup data bindings
		setupTimer();
		UpdateVolumeIcon(PlayerModel.Instance.currentVolume());
		ChangeRepeatLabel(QueueModel.Instance.repeat);

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

		QueueModel.Instance.OnRepeatChange += (repeat) =>
		{
			ChangeRepeatLabel(repeat);
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

	private void ChangeRepeatLabel(bool repeat)
	{
		if (repeat)
		{
			RepeatLabel.Background = new BrushConverter().ConvertFrom("#d2b4de") as Brush;
			RepeatLabel.Content = "\uE8ED";
		}
		else
		{
			RepeatLabel.Content = "\uE8EE";
			RepeatLabel.Background = new BrushConverter().ConvertFrom("#cacfd2") as Brush;
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
		QueueModel.Instance.toggleRepeat();
	}
	private void ShuffleSongCallback()
	{
		QueueModel.Instance.shuffleQueue();
	}
	private void NewPlaylistCallback()
	{
		PlaylistManager.Instance.CreatePlaylist();
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
		SelectedList.Name = Playlists[SelectedIndex].Name;
		SelectedList.Clear();
		foreach (var song in Playlists[SelectedIndex].Songs)
		{
			SelectedList.Add(new(song));
		}
	}
	public void DeletePlaylist(int index)
	{
		if (index < 0 || index >= Playlists.Count) return;
		PlaylistManager.Instance.RemovePlaylist(Playlists[index].Name);
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
}
