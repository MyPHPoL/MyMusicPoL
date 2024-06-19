using System.Text.Json.Serialization;
using MusicBackend.Interfaces;
using MusicBackend.Utils;

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
    Repeat,
    RandomLoop,
    Single,
}

public class QueueModel
{
    private List<Song> QueuedSongs { get; set; } = new();
    private List<int>? randomQueueIndexes = null;
    private List<int> RandomQueueIndexes
    {
        get
        {
            if (randomQueueIndexes is null)
            {
                randomQueueIndexes = Enumerable
                    .Range(0, QueuedSongs.Count)
                    .ToList();
                Shuffle.ShuffleList(randomQueueIndexes, Random.Shared);
            }
            return randomQueueIndexes;
        }
    }

    public IEnumerable<Song> Songs =>
        QueueMode is not QueueMode.RandomLoop
            ? QueuedSongs.AsEnumerable()
            : RandomQueueIndexes.Select(i => QueuedSongs[i]);

    private int current = 0;
    public QueueMode QueueMode { get; private set; }
    public event Action OnQueueModified = delegate { };
    public event Action<Song?> OnSongChange = delegate { };
    public event Action<QueueMode> OnQueueModeChange = delegate { };
    public event Action<Song>? OnSongChangeWhenRemoved;

    public event Action OnSkip = delegate { };

    private static QueueModel? _instance;
    public static QueueModel Instance
    {
        get => _instance ??= new QueueModel();
    }

    private QueueModel() { }

    private QueueModel(QueueModelState qms)
    {
        QueuedSongs = qms.songs;

        if (qms.current > qms.songs.Count)
            current = 0;
        else
            current = qms.current;
        QueueMode = qms.queueMode;
        FixupSongs();
    }

    internal static void InitWithState(QueueModelState qms)
    {
        _instance = new QueueModel(qms);
    }

    private void FixupSongs()
    {
        var currentCount = QueuedSongs.Count;
        QueuedSongs.RemoveAll(s => string.IsNullOrWhiteSpace(s.path));
        if (currentCount != QueuedSongs.Count)
        {
            current = 0;
        }
    }

    public void SetQueueMode(QueueMode mode)
    {
        var prevMode = QueueMode;
        QueueMode = mode;
        OnQueueModeChange(mode);
        if (
            mode == QueueMode.RandomLoop && prevMode != QueueMode.RandomLoop
            || mode != QueueMode.RandomLoop && prevMode == QueueMode.RandomLoop
        )
        {
            randomQueueIndexes = null;
            OnQueueModified();
        }
    }

    internal QueueModelState DumpState()
    {
        return new QueueModelState()
        {
            songs = QueuedSongs,
            current = current,
            queueMode = QueueMode
        };
    }

    public void AppendSong(Song song)
    {
        QueuedSongs.Add(song);
        if (randomQueueIndexes is not null)
        {
            randomQueueIndexes.Add(QueuedSongs.Count - 1);
        }
        OnQueueModified();
    }

    private void RemoveSongAt(int index)
    {
        var song = CurrentSong();
        if (randomQueueIndexes is not null)
        {
            var normalIndex = randomQueueIndexes[index];
            randomQueueIndexes.RemoveAt(index);
            for (int i = 0; i < randomQueueIndexes.Count; i++)
            {
                if (randomQueueIndexes[i] > normalIndex)
                {
                    randomQueueIndexes[i]--;
                }
            }
            index = normalIndex;
        }
        QueuedSongs.RemoveAt(index);
        if (index <= current)
        {
            current--;
            if (current == -1)
                current = 0;
            if (QueuedSongs.Count == 0)
            {
                OnSongChange(null);
            }

            var curSong = CurrentSong();
            if (curSong == song)
            {
                current = 0;
            }
            curSong = CurrentSong();
            if (curSong is not null)
            {
                OnSongChangeWhenRemoved?.Invoke(curSong);
            }
        }
    }

    public void RemoveSong(int index)
    {
        if (index < 0 || QueuedSongs.Count <= index)
            return;
        RemoveSongAt(index);
        OnQueueModified();
    }

    public void Clear()
    {
        QueuedSongs.Clear();
        randomQueueIndexes = null;
        current = 0;
        OnQueueModified();
        OnSongChange(null);
    }

    public Song? CurrentSong()
    {
        if (QueuedSongs.Count == 0)
            return null;
        return QueueMode == QueueMode.RandomLoop
            ? QueuedSongs[RandomQueueIndexes[current]]
            : QueuedSongs[current];
    }

    public void PlayNth(int index)
    {
        if (index < 0 || QueuedSongs.Count < index)
            return;
        current = index;
        OnSongChange(CurrentSong());
        OnSkip();
    }

    public void PlayPlaylist(string playlistName)
    {
        var playlist = PlaylistManager.Instance.GetPlaylist(playlistName);
        // if song was paused and we tried to play empty playlist then playlist wouldn't be null, but it would have 0 songs
        // bandaid fix ¯\_(ツ)_/¯
        if (playlist is null || playlist.Songs.Count == 0)
            return;
        QueuedSongs.Clear();
        foreach (var song in playlist.Songs)
        {
            QueuedSongs.Add(song);
        }
        current = 0;
        randomQueueIndexes = null;
        OnQueueModified();
        OnSongChange(CurrentSong());
    }

    internal Song? NextSong()
    {
        switch (QueueMode)
        {
            case QueueMode.Loop:
                current++;
                if (current == QueuedSongs.Count)
                {
                    current = 0;
                }
                return CurrentSong();
            case QueueMode.Single:
                current++;
                if (current == QueuedSongs.Count)
                {
                    current = 0;
                    return null;
                }
                return CurrentSong();
            case QueueMode.RandomLoop:
                current++;
                if (current == QueuedSongs.Count)
                {
                    current = 0;
                    Shuffle.ShuffleList(randomQueueIndexes!, Random.Shared);
                }
                return CurrentSong();
            case QueueMode.Repeat:
                return CurrentSong();
        }
        throw new InvalidOperationException("Invalid QueueMode");
    }

    public void ForceNextSong()
    {
        if (QueueMode == QueueMode.Repeat)
            SetQueueMode(QueueMode.Loop);
        if (QueuedSongs.Count == 0)
        {
            return;
        }
        current++;
        if (current == QueuedSongs.Count)
        {
            current = 0;
        }
        OnSongChange(CurrentSong());
        OnSkip();
    }

    public void ForcePrevSong()
    {
        if (QueueMode == QueueMode.Repeat)
            SetQueueMode(QueueMode.Loop);
        if (QueuedSongs.Count == 0)
        {
            return;
        }
        current--;
        if (current == -1)
        {
            current = QueuedSongs.Count - 1;
        }
        OnSongChange(CurrentSong());
        OnSkip();
    }
    //private IEnumerable<Song> GetRandomEnumerable()
    //{
    //    foreach (var index in RandomQueueIndexes)
    //    {
    //        yield return QueuedSongs[index];
    //    }
    //}
}
