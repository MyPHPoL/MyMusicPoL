using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MusicBackend.Model;

[DataContract]
internal class QueueModelState
{
	[JsonInclude]
	public List<Song> songs { get; set; } = new();
	[JsonInclude]
	public int current { get; set; } = 0;
	[JsonInclude]
	public bool repeat { get; set; } = false;
}

public class QueueModel
{
	public List<Song> songs { get; private set; } = new();
	int current = 0;
	public int Current { get => current; }
	public bool repeat { get; private set; } = false;
	public event Action OnQueueModified = delegate { };
	public event Action<Song?> OnSongChange = delegate { };
	public event Action<bool> OnRepeatChange = delegate { };

	private static QueueModel? _instance;
	public static QueueModel Instance { get => _instance ??= new QueueModel(); }

	private QueueModel()
	{
	}

	internal static void InitWithState(QueueModelState qms)
	{
		_instance = new QueueModel(qms);
	}
	private QueueModel (QueueModelState qms)
	{
		songs = qms.songs;

		if (qms.current > qms.songs.Count)
			current = 0;
		else
			current = qms.current;
		repeat = qms.repeat;
		fixupSongs();
	}
	private void fixupSongs()
	{
		var currentCount = songs.Count;
		songs.RemoveAll(s => string.IsNullOrWhiteSpace(s.path));
		if (currentCount != songs.Count)
		{
			current = 0;
		}
	}
	public void toggleRepeat()
	{
		repeat = !repeat;
		OnRepeatChange(repeat);
	}

	internal QueueModelState DumpState()
	{
		return new QueueModelState()
		{
			songs = songs,
			current = current,
			repeat = repeat,
		};
	}

	public bool isEmpty() => songs.Count == 0;

	public Song songAt(int index)
	{
		return songs[index];
	}
	public void appendSong(Song song)
	{
		songs.Add(song);
		OnQueueModified();
	}
	//public void appendSongByPath(string path)
	//{
	//	songs.Add(Song.fromPath(path));
	//}

	private void removeSongAt(int index)
	{
		var song = songs[index];
		songs.RemoveAt(index);
		if (index <= current)
		{
			current--;
			if (songs.Count == 0)
			{
				OnSongChange(null);
			}

			var curSong = currentSong();
			if (curSong == song)
			{
				current = 0;
			}
			if (currentSong() is not null)
			{
				OnSongChange(songs[current]);
			}
		}
	}


	public void removeSong(Song song)
	{
		var index = songs.Select((p, i) => (p, i)).First(p => p.p == song).i;
		if (index < 0 || songs.Count <= index) return;
		removeSongAt(index);
		OnQueueModified();
	}
	public void removeSong(int index)
	{
		if (index < 0 || songs.Count <= index) return;
		removeSongAt(index);
		OnQueueModified();
	}
	public Song? currentSong()
	{
		return songs.Count == 0 ? null : songs[current];
	}
	private static Random rng = new Random();
	public void shuffleQueue()
	{
		//dont shuffle empty queue and dont invoke callbacks
		if (songs.Count <= 1) return;
		// Fisher-Yates shuffle
		for (int i = songs.Count-1; i != 0; --i)
		{
			var j = rng.Next(i + 1);
			(songs[i], songs[j]) = (songs[j], songs[i]);
		}
		OnQueueModified();
	}
	public void playNth(int index)
	{
		if (index < 0 || songs.Count < index) return;
		current = index;
		OnSongChange(songs[current]);
	}
	public void forceNextSong()
	{
		if (songs.Count == 0)
		{
			return ;
		}
		current++;
		if (current == songs.Count)
		{
			current = 0;
		}
		OnSongChange(songs[current]);
	} 
	public void nextSong()
	{
		if (repeat && songs.Count != 0)
		{
			OnSongChange(songs[current]);
		}
		else
		{
			forceNextSong();
		}
	} 
	public void forcePrevSong()
	{
		if (songs.Count == 0)
		{
			return;
		}
		current--;
		if (current == -1)
		{
			current = songs.Count-1;
		}
		OnSongChange(songs[current]);
	} 
}
