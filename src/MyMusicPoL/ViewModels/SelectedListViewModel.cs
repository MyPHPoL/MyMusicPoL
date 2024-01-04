using CommunityToolkit.Mvvm.Input;
using MusicBackend.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mymusicpol.ViewModels;
internal class SelectedListViewModel : ViewModelBase
{
	private string name;
	public string Name
	{
		get => name;
		set
		{
			name = value;
			OnPropertyChanged(nameof(Name));
		}
	}
	public ObservableCollection<SongViewModel> Items { get; set; } = new();
	public int SelectedIndex { get; set; }

	class LibraryObserver : ILibraryObserver
	{
		private readonly SelectedListViewModel selectedListViewModel;
		public LibraryObserver(SelectedListViewModel playerViewModel)
		{
			this.selectedListViewModel = playerViewModel;
		}
		public void OnLibraryChange()
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				selectedListViewModel.ShowLibrary();
			});
		}
	}


	class PlaylistObserver : IPlaylistObserver
	{
		private readonly SelectedListViewModel selectedListViewModel;
		public PlaylistObserver(SelectedListViewModel playerViewModel)
		{
			this.selectedListViewModel = playerViewModel;
		}
		public void OnNewPlaylist(string name)
		{
		}

		public void OnPlaylistChange(string name)
		{
			if (name == selectedListViewModel.Name)
			{
				var songs = PlaylistManager.Instance.GetPlaylist(name);
				if (songs is null) return;

				selectedListViewModel.Clear();
				foreach (var song in songs.Songs)
				{
					selectedListViewModel.Items.Add(new SongViewModel(song));
				}
				selectedListViewModel.Sort();
			}
		}

		public void OnPlaylistNameEdited(string oldName, string newName)
		{
			if (selectedListViewModel.Name == oldName)
			{
				selectedListViewModel.Name = newName;
			}
		}

		public void OnPlaylistRemoved(string name)
		{
			if (selectedListViewModel.Name == name)
			{
				selectedListViewModel.ClearAll();
			}
		}
	}

	public bool IsQueue { get => Name == "Queue"; }
	public bool IsLibrary { get => Name == "Library"; }

	public ICommand DefaultSortCommand { get; }
	public ICommand AlbumSortCommand { get; }
	public ICommand ArtistSortCommand { get; }

	private SortMode sortMode = SortMode.ByTitle;
	public enum SortMode
	{
		ByTitle,
		ByArtist,
		ByAlbum,
	}

	public SelectedListViewModel()
	{
		DefaultSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByTitle));
		AlbumSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByAlbum));
		ArtistSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByArtist));

		PlaylistManager.Instance.Subscribe(new PlaylistObserver(this));
		LibraryManager.Instance.Subscribe(new LibraryObserver(this));

		ShowQueue();
		QueueModel.Instance.OnQueueModified += () =>
		{
			ShowQueue();
		};
		QueueModel.Instance.OnSongChange += (song) =>
		{
			SelectedIndex = QueueModel.Instance.Current;
		};
	}

	public void Clear()
	{
		Items.Clear();
	}
	public void ClearName()
	{
		Name = "Nothing selected";
	}
	public void ClearAll()
	{
		Clear();
		ClearName();
	}
	public void Add(SongViewModel song)
	{
		Items.Add(song);
		Sort();
	}

	public void RemoveSong(int index)
	{
		if (index < 0 || index >= Items.Count) return;

		if (IsQueue)
		{
			QueueModel.Instance.removeSong(index);
		}
		else if (IsLibrary)
		{
			// DO NOTHING
		}
		else
		{
			PlaylistManager.Instance.RemoveSongFromPlaylist(Name, Items[index].path);
		}
	}

	public void PlayNth(int index)
	{
		if (Name == "Queue")
			QueueModel.Instance.playNth(index);
	}
	
	public void AddToQueue(int index)
	{
		if (index < 0 || index >= Items.Count) return;
		var song = Song.fromPath(Items[index].path);
		if (song is null) return;
		QueueModel.Instance.appendSong(song);
	}

	public void AddToPlaylist(string playlistName, int index)
	{
		if (index < 0 || index >= Items.Count) return;

		PlaylistManager.Instance.AddSongToPlaylist(playlistName, Items[index].path);
	}

	public void AddSongs(SongViewModel[] songs)
	{
		foreach (var song in songs)
		{
			Items.Add(song);
		}
		Sort();
	}

	public void SetSortMode(SortMode mode)
	{
		sortMode = mode;
		Sort();
	}

	private void Sort()
	{
		if (Name == "Queue") return;

		var query = sortMode switch
		{
			SortMode.ByArtist =>
				Items.OrderBy(song => song.artist),
			SortMode.ByAlbum =>
				Items.OrderBy(song => song.album),
			_ =>
				Items.OrderBy(song => song.title)
		};
		var sortedList = query.ToList();

		Items.Clear();
		foreach (var song in sortedList)
		{
			Items.Add(song);
		}
	}

	public void ShowLibrary()
	{
		Name = "Library";
		Items.Clear();
		foreach (var song in LibraryManager.Instance.Songs)
		{
			Items.Add(new SongViewModel(song.Value));
		}
		Sort();
	}
	public void ShowQueue()
	{
		Name = "Queue";
		Items.Clear();
		foreach (var song in QueueModel.Instance.songs)
		{
			Items.Add(new SongViewModel(song));
		}
	}
}
