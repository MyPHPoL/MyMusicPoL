using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Model;
public interface IPlaylistObserver
{
	void OnPlaylistChange(string name);
	void OnNewPlaylist(string name);
	void OnPlaylistRemoved(string name);
	void OnPlaylistNameEdited(string oldName, string newName);
}
public class PlaylistManager
{

	private static PlaylistManager? instance;
	public static PlaylistManager Instance { get => instance ??= new(); }

	public SortedDictionary<string,Playlist> Playlists { get; private set; } = new();
	private List<IPlaylistObserver> _observers = new();
	private object _observerLock = new();

	private PlaylistManager()
	{
	}

	public void Subscribe(IPlaylistObserver observer)
	{
		lock(_observerLock)
		{
			_observers.Add(observer);
		}
	}
	
	public void Unsubscribe(IPlaylistObserver observer)
	{
		lock(_observerLock)
		{
			_observers.Remove(observer);
		}
	}

	private void NotifyPlaylistChange(string name)
	{
		lock (_observerLock)
		{
			foreach (var observer in _observers)
			{
				observer.OnPlaylistChange(name);
			}
		}
	}
	private void NotifyNewPlaylist(string name)
	{
		lock(_observerLock)
		{
			foreach (var observer in _observers)
			{
				observer.OnNewPlaylist(name);
			}
		}
	}
	private void NotifyPlaylistRemoved(string name)
	{
		lock(_observerLock)
		{
			foreach (var observer in _observers)
			{
				observer.OnPlaylistRemoved(name);
			}
		}
	}
	private void NotifyPlaylistEdited(string oldName, string newName)
	{
		lock(_observerLock)
		{
			foreach (var observer in _observers)
			{
				observer.OnPlaylistNameEdited(oldName,newName);
			}
		}
	}

	public void RemovePlaylist(string name)
	{
		bool status = Playlists.Remove(name);
		if (status is true)
		{
			NotifyPlaylistRemoved(name);
		}
	}

	public void CreatePlaylist()
	{
		var name = "Playlist " + Playlists.Count;
		bool status = Playlists.TryAdd(name,new Playlist(name));
		if (status is true)
		{
			NotifyNewPlaylist(name);
		}
	}
	public void EditPlaylistName(string oldName, string newName)
	{
		if (oldName == newName) return;
		if (newName == "Library" || newName == "Queue") return;
		bool status = Playlists.Remove(oldName,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			Playlists.Add(newName,playlist);
			NotifyPlaylistEdited(oldName, newName);
		}
	}

	public void AddSongToPlaylist(string playlistName,string songPath)
	{
		bool status = Playlists.TryGetValue(playlistName,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			if (playlist.Add(songPath) == true)
			{
				NotifyPlaylistChange(playlistName);
			}
		}
	}
	public void RemoveSongFromPlaylist(string playlistName,string songPath)
	{
		bool status = Playlists.TryGetValue(playlistName,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			if (playlist.Remove(songPath) == true)
			{
				NotifyPlaylistChange(playlistName);
			}
		}
	}
	public void RemoveSongFromPlaylistAt(string playlistName,int index)
	{
		bool status = Playlists.TryGetValue(playlistName,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			if (playlist.RemoveAt(index) == true)
			{
				NotifyPlaylistChange(playlistName);
			}
		}
	}

	public Playlist? GetPlaylist(string name)
	{
		bool status = Playlists.TryGetValue(name,out Playlist? playlist);
		return status is true ? playlist : null;
	}

}
