using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Model;
public interface ILibraryObserver
{
	void OnLibraryChange();
}
public class LibraryManager
{
	private static LibraryManager? instance;
	public static LibraryManager Instance { get => instance ??= new(); }

	public Dictionary<string,Song> Songs { get; private set; } = new();
	private object _songsLock = new();
	private List<ILibraryObserver> _observers = new();
	private FileSystemWatcher fsWatcher = new();
	private object _observerLock = new();

	private LibraryManager()
	{
		var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
		// initialize fs watcher
		fsWatcher.Path = musicPath;
		fsWatcher.IncludeSubdirectories = true;
		fsWatcher.NotifyFilter = System.IO.NotifyFilters.FileName;
		fsWatcher.Filter = "*.mp3";
		fsWatcher.Created += FsWatcher_Created;
		fsWatcher.Deleted += FsWatcher_Deleted;
		fsWatcher.Renamed += FsWatcher_Renamed;
		fsWatcher.EnableRaisingEvents = true;

		// read all files from subdirectories
		var files = System.IO.Directory.GetFiles(musicPath, "*.mp3", System.IO.SearchOption.AllDirectories);

		foreach (var file in files)
		{
			AddToLibrary(file);
		}
	}

	public void Subscribe(ILibraryObserver observer)
	{
		lock (_observerLock)
		{
			_observers.Add(observer);
		}
	}

	public void Unsubscribe(ILibraryObserver observer)
	{
		lock (_observerLock)
		{
			_observers.Remove(observer);
		}
	}

	public void NotifyLibraryChange()
	{
		lock (_observerLock)
		{
			foreach (var observer in _observers)
			{
				observer.OnLibraryChange();
			}
		}
	}

	private bool AddToLibrary(string path)
	{
		lock (_songsLock)
		{
			var song = Song.fromPath(path);
			if (song is null) return false;
			if (Songs.TryAdd(song.path, song) == false) return false;
		}

		NotifyLibraryChange();
		return true;
	}

	private bool RemoveFromLibrary(string path)
	{
		lock (_songsLock)
		{
			var song = Song.fromPath(path);
			if (song is null) return false;
			if (Songs.Remove(song.path) == false) return false;
		}

		NotifyLibraryChange();
		return true;
	}

	private bool RenameInLibrary(string oldPath, string newPath)
	{
		lock (_songsLock)
		{
			var song = Song.fromPath(oldPath);
			if (song is null) return false;
			if (Songs.Remove(oldPath) == false) return false;
			if (Songs.TryAdd(newPath, song) == false) return false;
		}

		NotifyLibraryChange();
		return true;
	}

	private void FsWatcher_Renamed(object sender, RenamedEventArgs e)
	{
		RenameInLibrary(e.OldFullPath, e.FullPath);
	}

	private void FsWatcher_Deleted(object sender, FileSystemEventArgs e)
	{
		RemoveFromLibrary(e.FullPath);
	}

	private void FsWatcher_Created(object sender, FileSystemEventArgs e)
	{
		AddToLibrary(e.FullPath);
	}
}
