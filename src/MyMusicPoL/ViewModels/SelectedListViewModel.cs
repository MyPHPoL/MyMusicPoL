using CommunityToolkit.Mvvm.Input;
using MusicBackend.Model;
using mymusicpol.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
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
	private Visibility _swapVisibility;
	public Visibility SwapVisibility
	{
		get => _swapVisibility;
		set
		{
			_swapVisibility = value;
			OnPropertyChanged(nameof(SwapVisibility));
		}
	}
	private Visibility _filterVisibility;
	public Visibility FilterVisiblity
	{
		get => _filterVisibility;
		set
		{
			_filterVisibility = value;
			OnPropertyChanged(nameof(FilterVisiblity));
		}
	}
	public ObservableCollection<SongViewModel> Items { get; set; } = new();

	public int _selectedIndex = -1;
	public int SelectedIndex
	{
		get => _selectedIndex;
		set 
		{
			_selectedIndex = value; OnPropertyChanged(nameof(SelectedIndex));
		}
	}

	public Notify<string> Filter { get; set; } = new();
	private Song[] originalItems = [];

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
				selectedListViewModel.ShowPlaylist(name);
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

	bool IsQueue { get => Name == "Queue"; }
	bool IsLibrary { get => Name == "Library"; }

	public ICommand DefaultSortCommand { get; }
	public ICommand AlbumSortCommand { get; }
	public ICommand ArtistSortCommand { get; }
	public ICommand TimeSortCommand { get; }
	public ICommand TitleSortCommand { get; }
	public ICommand SwapDownCommand { get;  }
	public ICommand SwapUpCommand { get;  }

	private SortMode sortMode = SortMode.ByDefault;
	public enum SortMode
	{
		ByDefault,
		ByTitle,
		ByArtist,
		ByAlbum,
		ByTime
	}

	public SelectedListViewModel()
	{
		DefaultSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByDefault));
		AlbumSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByAlbum));
		ArtistSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByArtist));
		TimeSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByTime));
		TitleSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByTitle));
		SwapDownCommand = new RelayCommand(SwapDownCallback);
		SwapUpCommand = new RelayCommand(SwapUpCallback);

		PlaylistManager.Instance.Subscribe(new PlaylistObserver(this));
		LibraryManager.Instance.Subscribe(new LibraryObserver(this));

		Filter.PropertyChanged += FilterChanged;

		ShowQueue();
		QueueModel.Instance.OnQueueModified += () =>
		{
			if (IsQueue)
				ShowQueue();
		};
		//QueueModel.Instance.OnSongChange += (song) =>
		//{
		//	if (IsQueue)
		//		SelectedIndex = QueueModel.Instance.Current;
		//};
	}

	private void SwapUpCallback()
	{
		if (SelectedIndex == -1 || IsLibrary || IsQueue) return;
		var index = PlaylistManager.Instance.MoveSongUp(Name, SelectedIndex);
		if (index != -1)
		{
			SelectedIndex = index;
		}
	}

	private void SwapDownCallback()
	{
		if (SelectedIndex == -1 || IsLibrary || IsQueue) return;

		var index = PlaylistManager.Instance.MoveSongDown(Name, SelectedIndex);
		if (index != -1)
		{
			SelectedIndex = index;
		}
	}

	private void FilterChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (String.IsNullOrEmpty(Filter.Value))
		{
			Refresh();
			return;
		}

		if (!IsQueue)
		{
			List<Song> list = new ();
			foreach(var song in originalItems)
			{
				if (song.name.Contains(Filter.Value, StringComparison.OrdinalIgnoreCase)
					|| song.artist.Contains(Filter.Value, StringComparison.OrdinalIgnoreCase)
					|| song.Album.Name.Contains(Filter.Value, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(song);
				}
			}
			Items.Clear();
			foreach (var song in list)
			{
				Items.Add(new (song));
			}
			Sort(Items);
		}
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
		Sort(Items);
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
		{
			QueueModel.Instance.playNth(index);
		}
		else if (Name == "Library")
		{
			var song = Song.fromPath(Items[index].path);
			if (song is null) return;
			QueueModel.Instance.appendSong(song);
			ShowQueue();
		}
		else
		{
			QueueModel.Instance.PlayPlaylist(Name);
		}
			
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
		Sort(Items);
	}

	public void SetSortMode(SortMode mode)
	{
		sortMode = mode;
		if (sortMode == SortMode.ByDefault)
		{
			Refresh();
		}
		else
		{
			Sort(Items);
		}
	}

	private void Sort<T>(T arr) where T : IEnumerable<SongViewModel>
	{
		if (Name == "Queue") return;

		var query = sortMode switch
		{
			SortMode.ByTitle =>
				arr.OrderBy(song => song.title),
			SortMode.ByArtist =>
				arr.OrderBy(song => song.artist),
			SortMode.ByAlbum =>
				arr.OrderBy(song => song.album),
			SortMode.ByTime =>
				arr.OrderBy(song => song.duration),
			SortMode.ByDefault =>
				(IEnumerable<SongViewModel>)arr,
		};

		if (sortMode != SortMode.ByDefault)
		{
			var sortedList = query.ToList();
			Items.Clear();
			foreach (var song in sortedList)
			{
				Items.Add(song);
			}
		}
	}

	public void ExportPlaylist(string filename)
	{
		PlaylistManager.Instance.ExportPlaylist(Name, filename);
	}

	public void ImportPlaylist(string filename)
	{
		PlaylistManager.Instance.ImportPlaylist(filename);
	}

	private void Refresh()
	{
		if (IsQueue)
		{
			ShowQueue();
		}
		else if (IsLibrary)
		{
			ShowLibrary();
		}
		else
		{
			ShowPlaylist(Name);
		}
	}

	public void ShowPlaylist(string name)
	{
		Name = name;
		SwapVisibility = Visibility.Visible;
		FilterVisiblity = Visibility.Visible;
		Items.Clear();
		var playlist = PlaylistManager.Instance.GetPlaylist(name);
		if (playlist is null) return;
		originalItems = playlist.Songs.ToArray();
		foreach (var song in playlist.Songs)
		{
			Items.Add(new SongViewModel(song));
		}
		Sort(Items);
	}

	public void ShowLibrary()
	{
		Name = "Library";
		SwapVisibility = Visibility.Collapsed;
		FilterVisiblity = Visibility.Visible;
		Items.Clear();
		originalItems = LibraryManager.Instance.Songs.Values.ToArray();
		foreach (var song in LibraryManager.Instance.Songs)
		{
			Items.Add(new SongViewModel(song.Value));
		}
		Sort(Items);
	}
	public void ShowQueue()
	{
		Name = "Queue";
		SwapVisibility = Visibility.Collapsed;
		FilterVisiblity = Visibility.Collapsed;
		Items.Clear();
		foreach (var song in QueueModel.Instance.songs)
		{
			Items.Add(new SongViewModel(song));
		}
	}
}
