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

		private int time_elapsed; // current song time elapsed
		public int Time_Elapsed
		{
			get { return time_elapsed; }
			set
			{
				time_elapsed = value;
				OnPropertyChanged(nameof(Time_Elapsed));
			}
		}

		private int time_remaining; // current song time remaining
		public int Time_Remaining
		{
			get { return time_remaining; }
			set
			{
				time_remaining = value;
				OnPropertyChanged(nameof(Time_Remaining));
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
		private int progress_value; // current song progress value (slider bar)
		public int Progress_Value
		{
			get { return progress_value; }
			set
			{
				progress_value = value;
				OnPropertyChanged(nameof(Progress_Value));
			}
		}
		private int progress_maximum; // current song progress maximum (slider bar)
		public int Progress_Maximum
		{
			get { return progress_maximum; }
			set
			{
				progress_maximum = value;
				OnPropertyChanged(nameof(Progress_Maximum));
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
				new SonglistViewModel(new Songlist("Playlist 2", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 3", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 4", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 5", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 6", new List<Song>())),
				new SonglistViewModel(new Songlist("Playlist 7", new List<Song>())),
			];

			volume = 50;
			progress_maximum = 100;
			progress_value = 50;
			time_elapsed = 50;
			time_remaining = 50;
			// fix this cover path
			cover = "..\\..\\assets\\TEST-BOX-100px-100px.png";
			title = "Test Title";
			artist = "Test Artist";
		}

	}
}
