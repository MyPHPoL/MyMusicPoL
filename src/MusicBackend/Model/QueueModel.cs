﻿using MusicBackend.Interfaces;
using MusicBackend.Utils;
using System.Text.Json.Serialization;

namespace MusicBackend.Model;

internal class QueueModelState
{
	[JsonInclude]
	public List<Song> songs { get; set; } = new();
	[JsonInclude]
	public int current { get; set; } = 0;
	[JsonInclude]
	public QueueMode queueMode { get; set; } = QueueMode.Loop;
}

public enum QueueMode
{
	Loop,
	OneLoop,
	RandomLoop,
	Single,
}

public class QueueModel
{
	public List<Song> songs { get; private set; } = new();
	int current = 0;
	public int Current { get => current; }
	public QueueMode QueueMode { get; private set; }
	public event Action OnQueueModified = delegate { };
	public event Action<Song?> OnSongChange = delegate { };
	public event Action<QueueMode> OnQueueModeChange = delegate { };
	public event Action OnSkip = delegate { };

	private static QueueModel? _instance;
	public static QueueModel Instance { get => _instance ??= new QueueModel(); }

	private QueueModel()
	{
	}
	private QueueModel(QueueModelState qms)
	{
		songs = qms.songs;

		if (qms.current > qms.songs.Count)
			current = 0;
		else
			current = qms.current;
		QueueMode = qms.queueMode;
		fixupSongs();
	}
	internal static void InitWithState(QueueModelState qms)
	{
		_instance = new QueueModel(qms);
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

	public void SetQueueMode(QueueMode mode)
	{
		QueueMode = mode;
		OnQueueModeChange(mode);
	}

	public IIterator<Song> GetIterator()
	{
		return QueueMode switch
		{
			QueueMode.Loop => new LoopIterator(this),
			QueueMode.OneLoop => new OneLoopIterator(this),
			QueueMode.RandomLoop => new RandomLoopIterator(this),
			QueueMode.Single => new SingleIterator(this),
			_ => new LoopIterator(this),
		};
	}

	internal QueueModelState DumpState()
	{
		return new QueueModelState()
		{
			songs = songs,
			current = current,
			queueMode = QueueMode
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

	private void removeSongAt(int index)
	{
		var song = songs[index];
		songs.RemoveAt(index);
		if (index <= current)
		{
			current--;
			if (current == -1) current = 0;
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
	public void playNth(int index)
	{
		if (index < 0 || songs.Count < index) return;
		current = index;
		OnSongChange(songs[current]);
		OnSkip();
	}
	public void PlayPlaylist(string playlistName)
	{
		var playlist = PlaylistManager.Instance.GetPlaylist(playlistName);
		// if song was paused and we tried to play empty playlist then playlist wouldn't be null, but it would have 0 songs
		// bandaid fix ¯\_(ツ)_/¯
		if (playlist is null || playlist.Songs.Count == 0) return;
		songs.Clear();
		foreach (var song in playlist.Songs)
		{
			songs.Add(song);
		}
		current = 0;
		OnQueueModified();
		OnSongChange(songs[current]);
	}
	public void forceNextSong()
	{
		if (QueueMode == QueueMode.OneLoop) SetQueueMode(QueueMode.Loop);
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
		OnSkip();
	} 
	public void forcePrevSong()
	{
		if (QueueMode == QueueMode.OneLoop) SetQueueMode(QueueMode.Loop);
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
		OnSkip();
	} 
	internal class LoopIterator : IIterator<Song>
	{
		private readonly QueueModel queueModel;
		private int index;
		public LoopIterator(QueueModel queueModel)
		{
			this.queueModel = queueModel;
			index = queueModel.current;
		}
		public bool HasNext()
		{
			return queueModel.songs.Count != 0;
		}

		public Song Next()
		{
			queueModel.forceNextSong();
			var song = queueModel.songs[queueModel.current];
			return song;
		}
	}
	internal class SingleIterator : IIterator<Song>
	{
		private readonly QueueModel queueModel;
		public SingleIterator(QueueModel queueModel)
		{
			this.queueModel = queueModel;
		}
		public bool HasNext()
		{
			return false;
		}

		public Song Next()
		{
			return queueModel.songs[queueModel.current];
		}
	}
	internal class OneLoopIterator : IIterator<Song>
	{
		private readonly QueueModel queueModel;
		private Song song;
		public OneLoopIterator(QueueModel queueModel)
		{
			this.queueModel = queueModel;
			song = queueModel.songs[queueModel.current];
		}
		public bool HasNext()
		{
			return true;
		}

		public Song Next()
		{
			return song;
		}
	}

	internal class RandomLoopIterator : IIterator<Song>
	{
		private readonly QueueModel queueModel;
		private readonly Random rng = new Random();
		private readonly List<int> indices = new();

		public RandomLoopIterator(QueueModel queueModel)
		{
			this.queueModel = queueModel;
			shuffle();
		}
		public bool HasNext()
		{
			return queueModel.songs.Count != 0;
		}
		public Song Next()
		{
			var song = queueModel.songs[indices[0]];
			indices.RemoveAt(0);
			if (indices.Count == 0)
			{
				shuffle();
			}
			return song;
		}
		private void shuffle()
		{
			for (int i = 0; i < queueModel.songs.Count; i++)
			{
				indices.Add(i);
			}
			Shuffle.ShuffleList(indices, rng);
		}
	}
}
