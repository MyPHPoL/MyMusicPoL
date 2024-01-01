using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Timers;
using MusicBackend.Utils;
using NAudio.Extras;
using NAudio.Wave;

namespace MusicBackend.Model;

public class PlayerModelState
{
	public float volume { get; set; } = 0.5F;
	public TimeSpan currentTime { get; set; } = TimeSpan.Zero;
	public string? currentSong { get; set; } = null;
}
public enum PlaybackState { Stopped, Playing, Paused }

public static class PlaybackStateExtensions
{
	public static bool isPlaying(this PlaybackState ps)
	{
		return ps == PlaybackState.Playing;
	}
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
	//private List<SampleObserver> sampleObservers = new();
	public event Action<float[]>? SamplesAccumulated;
	AudioFileReader? audioFileReader;
	WaveOutEvent waveOut;
	SampleAccumulator? sampleAccumulator;
	const int BUFFER_SIZE = 8192;
	// workaround for weird design of stop events
	private string? fileNameToSet = null;

	private static PlayerModel? _instance;
	public static PlayerModel Instance
	{
		get => _instance ??= new PlayerModel();
	}

	public static void InitWithState(PlayerModelState a)
	{
		_instance = new PlayerModel(a);
	}

	private PlayerModel()
	{
		waveOut = new();
	}
	private PlayerModel(PlayerModelState a)
	{
		waveOut = new();
		waveOut.Volume = a.volume;
		try
		{
			if (!string.IsNullOrEmpty(a.currentSong))
			{
				waveInit(a.currentSong);
			}
			audioFileReader.CurrentTime = a.currentTime;
		}
		catch { }
	}

	//public void Subscribe(SampleObserver observer)
	//{
	//	{ sampleObservers.Add(observer); }
	//}
	//public void Unsubscribe(SampleObserver observer)
	//{
	//		sampleObservers.Remove(observer);
	//}
	//private void NotifySamples(float[] samples)
	//{
	//		foreach (var observer in sampleObservers)
	//		{
	//			observer.SamplesNotify(samples);
	//		}
	//}

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

	public int BufferedSamplesLength()
	{
		return BUFFER_SIZE;
	}

/*	public bool TryReadSamples(ref byte[] samples)
	{
		if (is null)
		{
			return false;
		}
		bufferedWaveProvider.Read(samples, 0, samples.Length);
		return true;
	}
*/
	public PlayerModelState DumpState()
	{
		var ct = audioFileReader?.CurrentTime ?? TimeSpan.Zero;
		ct = TimeSpan.FromSeconds(Math.Floor(ct.TotalSeconds));
		return new PlayerModelState()
		{
			volume = waveOut.Volume,
			currentTime = ct,
			currentSong = audioFileReader?.FileName ?? string.Empty
		};
	}

	public void attachOnSongEnd(Action fn)
	{
		//callback is shared for waveout.Stop() and end of file
		waveOut.PlaybackStopped += (arg1, arg2) =>
		{
			if (fileNameToSet is not null)
			{
				waveInit(fileNameToSet);
				fileNameToSet = null;
				play();
			}
			else
			{
				fn();
			}
		};
	}
	private void waveInit(string filename)
	{
		audioFileReader = new(filename);
		sampleAccumulator = new(audioFileReader, BUFFER_SIZE);
		sampleAccumulator.SamplesAccumulated +=
			(s, e) =>
			{
				SamplesAccumulated?.Invoke(e);
			};
		waveOut.Init(sampleAccumulator);
	}

	public void UpdateTime()
	{
		if (audioFileReader is not null)
			OnTimeChange(audioFileReader.CurrentTime);
	}
	// requires stopped playback

	public TimeSpan songLength()
	{
		if (audioFileReader is null) return TimeSpan.Zero;
		return audioFileReader.TotalTime;
	}

	public string currentSong()
	{
		if (audioFileReader is null) return string.Empty;
		return audioFileReader.FileName;
	}
	// returns with playback enabled
	public void selectSong(string name)
	{
		if (audioFileReader is null || waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
		{
			waveInit(name);
			//var fileReader = new AudioFileReader(name);
			//waveOut.Init(fileReader);
			//audioFileReader = fileReader;
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
		if (audioFileReader is null) return;
		waveOut.Pause();
		OnPlaybackChange(PlaybackState.Paused);
	}
	public void play()
	{
		if (audioFileReader is null) return;
		waveOut.Play();
		OnPlaybackChange(PlaybackState.Playing);
	}
	public PlaybackState playbackState()
	{
		return (PlaybackState)waveOut.PlaybackState;
	}
	public float currentVolume()
	{
		return waveOut.Volume;
	}
	public void addVolume(float delta)
	{
		var vol = float.Round(waveOut.Volume + delta, 2);
		if (vol >= 0 && vol <= 1.0F)
		{
			waveOut.Volume = vol;
			OnVolumeChange(vol);
		}
	}

	public void setVolume(float vol)
	{
		if (vol >= 0 && vol <= 1.0F)
		{
			waveOut.Volume = vol;
			OnVolumeChange(vol);
		}
	}
	public void setTime(TimeSpan time)
	{
		if (audioFileReader is null) return;
		if (time >= TimeSpan.Zero && time <= audioFileReader.TotalTime)
		{
			audioFileReader.CurrentTime = time;
			OnTimeChange(audioFileReader.CurrentTime);
		}
	}

	public void addTime(float delta)
	{
		if (audioFileReader is null) return;
		var ct = audioFileReader.CurrentTime + TimeSpan.FromSeconds(delta);
		//if (delta > 0 && audioFileReader.CurrentTime < audioFileReader.TotalTime
		// || delta < 0 && audioFileReader.CurrentTime > TimeSpan.FromSeconds(float.Abs(delta)))
		if (ct >= TimeSpan.Zero && audioFileReader.CurrentTime <= audioFileReader.TotalTime)
		{
			audioFileReader.CurrentTime = ct;
			OnTimeChange(audioFileReader.CurrentTime);
		}
	}
}
