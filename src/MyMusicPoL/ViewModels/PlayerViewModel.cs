using mymusicpol.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mymusicpol.ViewModels
{
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

		private string timeRemaining; // current song time remaining
		public string TimeRemaining
		{
			get { return timeRemaining; }
			set
			{
				timeRemaining = value;
				OnPropertyChanged(nameof(TimeRemaining));
			}
		}

		private int volume; // current volume
		public int Volume
		{
			get { return volume; }
			set
			{
				volume = value;
				OnPropertyChanged(nameof(Volume));
			}
		}

		private int progressValue; // current song progress value (slider bar)
		public int ProgressValue
		{
			get { return progressValue; }
			set
			{
				progressValue = value;
				OnPropertyChanged(nameof(ProgressValue));
			}
		}

		private int progressMaximum; // current song progress maximum (slider bar)
		public int ProgressMaximum
		{
			get { return progressMaximum; }
			set
			{
				progressMaximum = value;
				OnPropertyChanged(nameof(ProgressMaximum));
			}
		}

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
    
		private readonly ObservableCollection<SonglistViewModel> playlists; // all playlists
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
				new SonglistViewModel(new Songlist("Playlist 1", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 2 Different Name", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 3 LOLOLO", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 4 xdd", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 5 test", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 6 oho", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 7 haahahahahah", new List<Song>())),
			];

			volume = 50;
			progressMaximum = 100;
			progressValue = 50;
			timeElapsed = "0:00:00";
			timeRemaining = "-0:00:00";
			// IDK HOW TO SHOW THIS DAMN COVER
			cover = "assets\\TEST-BOX-100px-100px.png";
			title = "Test Title";
			artist = "Test Artist";
			selectedListName = "Test Playlist Name";
		}

	}
}
