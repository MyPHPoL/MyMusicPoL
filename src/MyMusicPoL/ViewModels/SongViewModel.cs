using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using mymusicpol.Models;
using mymusicpol.Views.Languages;

namespace mymusicpol.ViewModels;

internal class SongViewModel : INotifyPropertyChanged
{
    private string _title;
    private string _artist;
    private string _album;
    private BitmapSource _cover;
    private string _path;
    private TimeSpan _duration;
    public int Index { get; private set; }

    public SongViewModel(MusicBackend.Model.Song? song, int index)
    {
        SetSong(song);
        Index = index;
    }

    public SongViewModel AdjustIndex(int index)
    {
        Index = index;
        return this;
    }

    public void SetSong(MusicBackend.Model.Song? song)
    {
        if (song is not null)
        {
            this.title = song.name;
            this.artist = song.artist;
            this.album = song.Album.Name;
            this.path = song.path;
            this.duration = song.length;
        }
        else
        {
            this.title = Resources.noSong;
            this.artist = "";
            this.album = "";
            this.path = "";
            this.duration = TimeSpan.Zero;
        }
        var image = AlbumCoverManager.Instance.GetCover(song);
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

    public TimeSpan duration
    {
        get => _duration;
        set
        {
            _duration = value;
            OnPropertyChanged(nameof(duration));
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
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName)
        );
    }
    #endregion
}
