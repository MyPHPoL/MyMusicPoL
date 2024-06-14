using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicBackend.Model;
using mymusicpol.Utils;

namespace mymusicpol.ViewModels;

internal partial class SelectedListViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private Visibility _swapVisibility;

    [ObservableProperty]
    private Visibility _filterVisibility;

    [ObservableProperty]
    private Visibility _queueButtonVisibility;
    public ObservableCollection<SongViewModel> Items { get; set; } = new();

    [ObservableProperty]
    public int _selectedIndex = -1;

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

        public void OnNewPlaylist(string name) { }

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
                selectedListViewModel.ShowLibrary();
            }
        }
    }

    private enum ListType
    {
        Queue,
        Library,
        Playlist
    }

    ListType listType;
    bool IsQueue => listType == ListType.Queue;
    bool IsLibrary => listType == ListType.Playlist;

    public ICommand DefaultSortCommand { get; }
    public ICommand AlbumSortCommand { get; }
    public ICommand ArtistSortCommand { get; }
    public ICommand TimeSortCommand { get; }
    public ICommand TitleSortCommand { get; }
    public ICommand SwapDownCommand { get; }
    public ICommand SwapUpCommand { get; }

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
        DefaultSortCommand = new RelayCommand(
            () => SetSortMode(SortMode.ByDefault)
        );
        AlbumSortCommand = new RelayCommand(
            () => SetSortMode(SortMode.ByAlbum)
        );
        ArtistSortCommand = new RelayCommand(
            () => SetSortMode(SortMode.ByArtist)
        );
        TimeSortCommand = new RelayCommand(() => SetSortMode(SortMode.ByTime));
        TitleSortCommand = new RelayCommand(
            () => SetSortMode(SortMode.ByTitle)
        );
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
    }

    private void SwapUpCallback()
    {
        if (SelectedIndex == -1 || IsLibrary || IsQueue)
            return;
        var index = PlaylistManager.Instance.MoveSongUp(Name, SelectedIndex);
        if (index != -1)
        {
            SelectedIndex = index;
        }
    }

    private void SwapDownCallback()
    {
        if (SelectedIndex == -1 || IsLibrary || IsQueue)
            return;

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
            List<Song> list = new();
            var comparison = StringComparison.InvariantCultureIgnoreCase;
            foreach (var song in originalItems)
            {
                var splittedSongName = song.name.Split(" ");
                foreach (var songName in splittedSongName)
                {
                    if (
                        songName.StartsWith(Filter.Value, comparison)
                        && !list.Contains(song)
                    )
                    {
                        list.Add(song);
                    }
                }
            }
            Items.Clear();
            foreach (var song in list)
            {
                Items.Add(new(song));
            }
            Sort(Items);
        }
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void Add(SongViewModel song)
    {
        Items.Add(song);
        Sort(Items);
    }

    public void RemoveSong(int index)
    {
        if (index < 0 || index >= Items.Count)
            return;

        if (IsQueue)
        {
            QueueModel.Instance.RemoveSong(index);
        }
        else if (IsLibrary)
        {
            // DO NOTHING
        }
        else
        {
            PlaylistManager.Instance.RemoveSongFromPlaylist(
                Name,
                Items[index].path
            );
        }
    }

    public void PlayNth(int index)
    {
        if (IsQueue)
        {
            QueueModel.Instance.PlayNth(index);
        }
        else if (IsLibrary)
        {
            var song = Song.fromPath(Items[index].path);
            if (song is null)
                return;
            QueueModel.Instance.AppendSong(song);
            ShowQueue();
        }
        else
        {
            QueueModel.Instance.PlayPlaylist(Name);
        }
    }

    public void AddToQueue(int index)
    {
        if (index < 0 || index >= Items.Count)
            return;
        var song = Song.fromPath(Items[index].path);
        if (song is null)
            return;
        QueueModel.Instance.AppendSong(song);
    }

    public void AddToPlaylist(string playlistName, int index)
    {
        if (index < 0 || index >= Items.Count)
            return;

        PlaylistManager.Instance.AddSongToPlaylist(
            playlistName,
            Items[index].path
        );
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

    private void Sort<T>(T arr)
        where T : IEnumerable<SongViewModel>
    {
        if (IsQueue)
            return;

        var query = sortMode switch
        {
            SortMode.ByTitle => arr.OrderBy(song => song.title),
            SortMode.ByArtist => arr.OrderBy(song => song.artist),
            SortMode.ByAlbum => arr.OrderBy(song => song.album),
            SortMode.ByTime => arr.OrderBy(song => song.duration),
            SortMode.ByDefault => (IEnumerable<SongViewModel>)arr,
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
        listType = ListType.Playlist;
        SwapVisibility = Visibility.Visible;
        FilterVisibility = Visibility.Visible;
        QueueButtonVisibility = Visibility.Collapsed;
        Items.Clear();
        var playlist = PlaylistManager.Instance.GetPlaylist(name);
        if (playlist is null)
            return;
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
        listType = ListType.Library;
        SwapVisibility = Visibility.Collapsed;
        FilterVisibility = Visibility.Visible;
        QueueButtonVisibility = Visibility.Collapsed;
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
        listType = ListType.Queue;
        SwapVisibility = Visibility.Collapsed;
        FilterVisibility = Visibility.Collapsed;
        QueueButtonVisibility = Visibility.Visible;
        Items.Clear();
        foreach (var song in QueueModel.Instance.Songs)
        {
            Items.Add(new SongViewModel(song));
        }
    }
}
