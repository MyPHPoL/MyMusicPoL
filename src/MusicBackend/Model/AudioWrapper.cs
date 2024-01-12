using MusicBackend.Utils;
using NAudio.Wave;

namespace MusicBackend.Model;
public enum PlaybackState { Stopped, Playing, Paused }

public static class PlaybackStateExtensions
{
	public static bool isPlaying(this PlaybackState ps)
	{
		return ps == PlaybackState.Playing;
	}
}
internal class AudioWrapper : IDisposable
{
	//public event Action<PlaybackState> OnPlaybackChange = delegate { };
	//public event Action<float> OnVolumeChange = delegate { };
	//public event Action<TimeSpan> OnTimeChange = delegate { };
	//private List<SampleObserver> sampleObservers = new();
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
	public (int,int,int) WaveFormat()
	{
		if (sampleAccumulator is null)
		{
			return (0, 0, 0);
		}
		var wf = sampleAccumulator.WaveFormat;
		return (wf.SampleRate, wf.BitsPerSample, wf.Channels);
	}

	public void attachOnSongEnd(Action fn)
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
			play();
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
			sampleAccumulator = new(audioFileReader.ToSampleProvider(), BUFFER_SIZE);
			sampleAccumulator.SamplesAccumulated +=
				(s, e) =>
				{
					SamplesAccumulated?.Invoke(e);
				};
			waveOut.Init(sampleAccumulator);
		}
		catch
		{
			currentFileName = "";
			audioFileReader = null;
		}
	}
	public void reset()
	{
		audioFileReader = null;
		sampleAccumulator = null;
		waveOut.Stop();
	}

	public bool UpdateTime()
	{
		if (audioFileReader is not null)
			return true;
			//OnTimeChange(audioFileReader.CurrentTime);
		return false;
	}

	// requires stopped playback

	public TimeSpan songLength()
	{
		if (audioFileReader is null) return TimeSpan.Zero;
		return audioFileReader.TotalTime;
	}

	public string? currentSong()
	{
		if (audioFileReader is null) return null;
		return currentFileName;
	}
	/// <summary>
	/// Returns with playback enabled
	/// </summary>
	/// <param name="name">Name of the song to select</param>
	public void selectSong(string name)
	{
		if (audioFileReader is null || waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
		{
			waveInit(name);
			play();
		}
		else
		{
			fileNameToSet = name;
			waveOut.Stop();
		}
	}
	public TimeSpan currentTime()
	{
		if (audioFileReader is null) return TimeSpan.Zero;
		return audioFileReader.CurrentTime;
	}
	public void pause()
	{
		if (audioFileReader is null) return ;
		waveOut.Pause();
		OnPlaybackChange?.Invoke(PlaybackState.Paused);
	}
	public void play()
	{
		if (audioFileReader is null) return ;
		waveOut.Play();
		OnPlaybackChange?.Invoke(PlaybackState.Playing);
	}
	public PlaybackState playbackState()
	{
		return (PlaybackState)waveOut.PlaybackState;
	}
	public float currentVolume()
	{
		return waveOut.Volume;
	}
	public bool addVolume(float delta)
	{
		var vol = float.Round(waveOut.Volume + delta, 2);
		if (vol >= 0 && vol <= 1.0F)
		{
			waveOut.Volume = vol;
			return true;
			//OnVolumeChange(vol);
		}
		return false;
	}

	public bool setVolume(float vol)
	{
		if (vol >= 0 && vol <= 1.0F)
		{
			waveOut.Volume = vol;
			return true;
			//OnVolumeChange(vol);
		}
		return false;
	}
	public bool setTime(TimeSpan time)
	{
		if (audioFileReader is null) return false;
		if (time >= TimeSpan.Zero && time <= audioFileReader.TotalTime)
		{
			audioFileReader.CurrentTime = time;
			return true;
			//OnTimeChange(audioFileReader.CurrentTime);
		}
		return false;
	}

	public bool addTime(float delta)
	{
		if (audioFileReader is null) return false;
		var ct = audioFileReader.CurrentTime + TimeSpan.FromSeconds(delta);
		if (ct >= TimeSpan.Zero && audioFileReader.CurrentTime <= audioFileReader.TotalTime)
		{
			audioFileReader.CurrentTime = ct;
			return true;
			//OnTimeChange(audioFileReader.CurrentTime);
		}
		return false;
	}
}
