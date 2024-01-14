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
	public static string MusicPath { get => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); }

	public Dictionary<string,Song> Songs { get; private set; } = new();
	private object _songsLock = new();
	private List<ILibraryObserver> _observers = new();
	private FileSystemWatcher fsWatcher = new();
	private object _observerLock = new();

	private LibraryManager()
	{
		var musicPath = LibraryManager.MusicPath;
		// initialize fs watcher
		fsWatcher.Path = musicPath;
		fsWatcher.IncludeSubdirectories = true;
		fsWatcher.NotifyFilter = System.IO.NotifyFilters.FileName;
		fsWatcher.Filters.Add("*.mp3");
		fsWatcher.Filters.Add("*.wav");
		fsWatcher.Created += FsWatcher_Created;
		fsWatcher.Deleted += FsWatcher_Deleted;
		fsWatcher.Renamed += FsWatcher_Renamed;
		fsWatcher.EnableRaisingEvents = true;

		// read all files in directory and select mp3 and wav files
		var files = System.IO.Directory.EnumerateFiles(musicPath, "*.*", System.IO.SearchOption.AllDirectories)
			.Where(s =>
			{
				var ext = Path.GetExtension(s);
				return ext == ".mp3" || ext == ".wav";
			});

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
