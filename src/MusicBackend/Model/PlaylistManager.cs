using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicBackend.Model;
public interface IPlaylistObserver
{
	void OnPlaylistChange(string name);
	void OnNewPlaylist(string name);
	void OnPlaylistRemoved(string name);
	void OnPlaylistNameEdited(string oldName, string newName);
}


internal class PlaylistManagerState
{
	[JsonInclude]
	public PlaylistState[] Playlists { get; set; }
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

	private PlaylistManager(PlaylistManagerState state)
	{
		foreach (var playlistState in state.Playlists)
		{
			var playlist = new Playlist(playlistState);
			Playlists.Add(playlist.Name,playlist);
		}
	}

	internal static void InitWithState(PlaylistManagerState state)
	{
		instance = new PlaylistManager(state);
	}

	internal PlaylistManagerState DumpState()
	{
		var state = new PlaylistManagerState()
		{
			Playlists = Playlists.Select(x => x.Value.DumpState()).ToArray()
		};
		return state;
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

	public bool CreatePlaylist(string name)
	{
		bool status = Playlists.TryAdd(name,new Playlist(name));
		if (status is true)
		{
			NotifyNewPlaylist(name);
			return true;
		}
		return false;
	}
	public void EditPlaylistName(string oldName, string newName)
	{
		if (oldName == newName) return;
		if (newName == "Library" || newName == "Queue") return;
		if (Playlists.ContainsKey(newName)) return;

		bool status = Playlists.Remove(oldName,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			playlist.Name = newName;
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

	public void ExportPlaylist(string playlistName,string path)
	{
		bool status = Playlists.TryGetValue(playlistName,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			playlist.Export(path);
		}
	}

	public void ImportPlaylist(string path)
	{
		var playlist = Playlist.Import(path);
		if (playlist is not null)
		{
			var res = Playlists.TryAdd(playlist.Name,playlist);
			if (res == true)
			{
				NotifyNewPlaylist(playlist.Name);
				NotifyPlaylistChange(playlist.Name);
			}
		}
	}

	public Playlist? GetPlaylist(string name)
	{
		bool status = Playlists.TryGetValue(name,out Playlist? playlist);
		return status is true ? playlist : null;
	}

	/// <summary>
	/// Move song in a playlist one index up
	/// </summary>
	/// <param name="name">Playlist to move song in</param>
	/// <param name="index">Index of the song to move</param>
	public int MoveSongUp(string name, int index)
	{
		bool status = Playlists.TryGetValue(name,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			var ret = playlist.MoveSongUp(index);
			NotifyPlaylistChange(name);
			return ret;
		}
		return -1;
	}

	/// <summary>
	/// Move song in a playlist one index down
	/// </summary>
	/// <param name="name">Playlist to move song in</param>
	/// <param name="index">Index of the song to move</param>
	public int MoveSongDown(string name, int index)
	{
		bool status = Playlists.TryGetValue(name,out Playlist? playlist);
		if (status is true && playlist is not null)
		{
			int ret = playlist.MoveSongDown(index);
			NotifyPlaylistChange(name);
			return ret;
		}
		return -1;
	}

}
