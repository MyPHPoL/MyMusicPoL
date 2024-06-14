using MusicBackend.Utils;
using NAudio.Wave;

namespace MusicBackend.Model;

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}

public static class PlaybackStateExtensions
{
    public static bool isPlaying(this PlaybackState ps)
    {
        return ps == PlaybackState.Playing;
    }
}

internal class AudioWrapper : IDisposable
{
    internal event Action<float[]>? SamplesAccumulated;
    internal Action<PlaybackState>? OnPlaybackChange = delegate { };
    private Action? OnSongEnd = delegate { };
    MediaFoundationReader? audioFileReader;
    private string currentFileName = "";
    WaveOutEvent waveOut;
    SampleAccumulator? sampleAccumulator;

    // workaround for weird design of stop events
    private string? fileNameToSet = null;

    // buffer size of sample accumulator
    internal const int BUFFER_SIZE = 4096;

    public AudioWrapper()
    {
        waveOut = new();
        waveOut.PlaybackStopped += OnSongEndCallback;
    }

    public AudioWrapper(float volume, TimeSpan currentTime, string? currentSong)
    {
        waveOut = new();
        waveOut.Volume = volume;
        waveOut.PlaybackStopped += OnSongEndCallback;
        try
        {
            if (!string.IsNullOrEmpty(currentSong))
            {
                waveInit(currentSong);
            }
            audioFileReader.CurrentTime = currentTime;
        }
        catch { }
    }

    public void Dispose()
    {
        audioFileReader?.Dispose();
        sampleAccumulator = null;
        waveOut.PlaybackStopped -= OnSongEndCallback;
        waveOut.Dispose();
        OnSongEnd = null;
        SamplesAccumulated = null;
    }

    /**
     * @brief For the currently playing song returns:
     * - sample rate
     * - bits per sample
     * - number of channels
     * all zero if no song is selected
     */
    public (int, int, int) WaveFormat()
    {
        if (sampleAccumulator is null)
        {
            return (0, 0, 0);
        }
        var wf = sampleAccumulator.WaveFormat;
        return (wf.SampleRate, wf.BitsPerSample, wf.Channels);
    }

    public void AttachOnSongEnd(Action fn)
    {
        //callback is shared for waveout.Stop() and end of file
        OnSongEnd += fn;
    }

    private void OnSongEndCallback(object? arg1, StoppedEventArgs arg2)
    {
        if (fileNameToSet is not null)
        {
            waveInit(fileNameToSet);
            fileNameToSet = null;
            Play();
        }
        else
        {
            OnSongEnd?.Invoke();
        }
    }

    private void waveInit(string filename)
    {
        try
        {
            audioFileReader = new(filename);
            currentFileName = filename;
            sampleAccumulator = new(
                audioFileReader.ToSampleProvider(),
                BUFFER_SIZE
            );
            sampleAccumulator.SamplesAccumulated += (s, e) =>
            {
                SamplesAccumulated?.Invoke(e);
            };
            waveOut.DesiredLatency = 100;
            waveOut.Init(sampleAccumulator);
        }
        catch
        {
            currentFileName = "";
            audioFileReader = null;
        }
    }

    public void Reset()
    {
        audioFileReader = null;
        sampleAccumulator = null;
        waveOut.Stop();
    }

    public bool UpdateTime()
    {
        if (audioFileReader is not null)
            return true;
        return false;
    }

    // requires stopped playback

    public TimeSpan SongLength()
    {
        if (audioFileReader is null)
            return TimeSpan.Zero;
        return audioFileReader.TotalTime;
    }

    public string? CurrentSong()
    {
        if (audioFileReader is null)
            return null;
        return currentFileName;
    }

    /// <summary>
    /// Returns with playback enabled
    /// </summary>
    /// <param name="name">Name of the song to select</param>
    public void SelectSong(string name)
    {
        if (
            audioFileReader is null
            || waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped
        )
        {
            waveInit(name);
            Play();
        }
        else
        {
            fileNameToSet = name;
            waveOut.Stop();
        }
    }

    public TimeSpan CurrentTime()
    {
        if (audioFileReader is null)
            return TimeSpan.Zero;
        return audioFileReader.CurrentTime;
    }

    public void Pause()
    {
        if (audioFileReader is null)
            return;
        waveOut.Pause();
        OnPlaybackChange?.Invoke(Model.PlaybackState.Paused);
    }

    public void Play()
    {
        if (audioFileReader is null)
            return;
        waveOut.Play();
        OnPlaybackChange?.Invoke(Model.PlaybackState.Playing);
    }

    public PlaybackState PlaybackState()
    {
        return (PlaybackState)waveOut.PlaybackState;
    }

    public float CurrentVolume()
    {
        return waveOut.Volume;
    }

    public bool AddVolume(float delta)
    {
        var vol = float.Round(waveOut.Volume + delta, 2);
        if (vol >= 0 && vol <= 1.0F)
        {
            waveOut.Volume = vol;
            return true;
        }
        return false;
    }

    public bool SetVolume(float vol)
    {
        if (vol >= 0 && vol <= 1.0F)
        {
            waveOut.Volume = vol;
            return true;
        }
        return false;
    }

    public bool SetTime(TimeSpan time)
    {
        if (audioFileReader is null)
            return false;
        if (time >= TimeSpan.Zero && time <= audioFileReader.TotalTime)
        {
            audioFileReader.CurrentTime = time;
            return true;
        }
        return false;
    }

    public bool AddTime(float delta)
    {
        if (audioFileReader is null)
            return false;
        var ct = audioFileReader.CurrentTime + TimeSpan.FromSeconds(delta);
        if (
            ct >= TimeSpan.Zero
            && audioFileReader.CurrentTime <= audioFileReader.TotalTime
        )
        {
            audioFileReader.CurrentTime = ct;
            return true;
        }
        return false;
    }
}
