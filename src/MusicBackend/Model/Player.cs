using System.Text.Json.Serialization;
using MusicBackend.Interfaces;

namespace MusicBackend.Model;

internal class PlayerModelState
{
    [JsonInclude]
    public float volume { get; set; } = 0.5F;

    [JsonInclude]
    public TimeSpan currentTime { get; set; } = TimeSpan.Zero;

    [JsonInclude]
    public string? currentSong { get; set; } = null;
}

public interface SampleObserver
{
    public void SamplesNotify(float[] samples);
}

public class PlayerModel
{
    public event Action<PlaybackState> OnPlaybackChange = delegate { };
    public event Action<float> OnVolumeChange = delegate { };
    public event Action<TimeSpan> OnTimeChange = delegate { };
    public event Action<Song?> OnSongChange = delegate { };

    //private List<SampleObserver> sampleObservers = new();
    internal AudioWrapper audioWrapper;

    private static PlayerModel? _instance;
    public static PlayerModel Instance
    {
        get => _instance ??= new PlayerModel();
    }

    internal static void InitWithState(PlayerModelState a)
    {
        _instance = new PlayerModel(a);
    }

    private PlayerModel()
    {
        audioWrapper = new();
        AttachCallbacks();
    }

    private PlayerModel(PlayerModelState a)
    {
        audioWrapper = new(a.volume, a.currentTime, a.currentSong);
        AttachCallbacks();
    }

    private void AttachCallbacks()
    {
        audioWrapper.OnPlaybackChange += (ps) => OnPlaybackChange(ps);
        AttachOnSongEnd(OnSongEnd);
        QueueModel.Instance.OnSongChange += (s) =>
        {
            SelectSong(s?.path);
        };
        QueueModel.Instance.OnSongChangeWhenRemoved += (s) =>
        {
            if (PlaybackState() == Model.PlaybackState.Playing)
            {
                SelectSong(s.path);
            }
        };
    }

    private void OnSongEnd()
    {
        var song = QueueModel.Instance.NextSong();
        if (song is not null)
        {
            SelectSong(song.path);
            OnSongChange(song);
        }
        else
        {
            audioWrapper.Reset();
            OnPlaybackChange(Model.PlaybackState.Stopped);
        }
    }

    internal PlayerModelState DumpState()
    {
        var ct = audioWrapper.CurrentTime();
        ct = TimeSpan.FromSeconds(Math.Floor(ct.TotalSeconds));
        return new PlayerModelState()
        {
            volume = audioWrapper.CurrentVolume(),
            currentTime = ct,
            currentSong = audioWrapper.CurrentSong()
        };
    }

    public void AttachOnSongEnd(Action fn)
    {
        audioWrapper.AttachOnSongEnd(fn);
    }

    public void UpdateTime()
    {
        if (audioWrapper.UpdateTime())
            OnTimeChange(audioWrapper.CurrentTime());
    }

    public TimeSpan SongLength()
    {
        return audioWrapper.SongLength();
    }

    public string? CurrentSong()
    {
        return audioWrapper.CurrentSong();
    }

    // returns with playback enabled
    public void SelectSong(string? name)
    {
        if (name is null)
        {
            audioWrapper.Reset();
        }
        else
        {
            audioWrapper.SelectSong(name);
        }
    }

    public TimeSpan CurrentTime()
    {
        return audioWrapper.CurrentTime();
    }

    public void Pause()
    {
        audioWrapper.Pause();
    }

    public void Play()
    {
        audioWrapper.Play();
    }

    public PlaybackState PlaybackState()
    {
        return audioWrapper.PlaybackState();
    }

    public float CurrentVolume()
    {
        return audioWrapper.CurrentVolume();
    }

    public void AddVolume(float delta)
    {
        if (audioWrapper.AddVolume(delta) is true)
            OnVolumeChange(audioWrapper.CurrentVolume());
    }

    public void SetVolume(float vol)
    {
        if (audioWrapper.SetVolume(vol) is true)
            OnVolumeChange(audioWrapper.CurrentVolume());
    }

    public void SetTime(TimeSpan time)
    {
        if (audioWrapper.SetTime(time) is true)
            OnTimeChange(audioWrapper.CurrentTime());
    }

    public void AddTime(float delta)
    {
        if (audioWrapper.AddTime(delta) is true)
            OnTimeChange(audioWrapper.CurrentTime());
    }
}
