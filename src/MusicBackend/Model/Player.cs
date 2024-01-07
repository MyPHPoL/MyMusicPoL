using MusicBackend.Interfaces;
using System.Text.Json.Serialization;

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
	private IIterator<Song> queueIterator;

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
		attachCallbacks();
	}
	private PlayerModel(PlayerModelState a)
	{
		audioWrapper = new (a.volume, a.currentTime, a.currentSong);
		attachCallbacks();
	}

	private void attachCallbacks()
	{
		audioWrapper.OnPlaybackChange += (ps) => OnPlaybackChange(ps);
		attachOnSongEnd(OnSongEnd);
		QueueModel.Instance.OnQueueModified += () => refreshIterator();
		QueueModel.Instance.OnSongChange += (s) =>
		{
			selectSong(s?.path);
		};
		QueueModel.Instance.OnRepeatChange += (q) => refreshIterator();
		QueueModel.Instance.OnSkip += () => refreshIterator();
	}

	private void refreshIterator() => queueIterator = QueueModel.Instance.GetIterator();

	private void OnSongEnd()
	{
		if (queueIterator.HasNext())
		{
			var song = queueIterator.Next();
			selectSong(song.path);
			OnSongChange(song);
		}
		else
		{
			audioWrapper.reset();
			OnPlaybackChange(PlaybackState.Stopped);
		}
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
	internal PlayerModelState DumpState()
	{
		var ct = audioWrapper.currentTime();
		ct = TimeSpan.FromSeconds(Math.Floor(ct.TotalSeconds));
		return new PlayerModelState()
		{
			volume = audioWrapper.currentVolume(),
			currentTime = ct,
			currentSong = audioWrapper.currentSong()
		};
	}

	public void attachOnSongEnd(Action fn)
	{
		audioWrapper.attachOnSongEnd(fn);
	}

	public void UpdateTime()
	{
		if (audioWrapper.UpdateTime())
			OnTimeChange(audioWrapper.currentTime());
	}
	// requires stopped playback

	public TimeSpan songLength()
	{
		return audioWrapper.songLength();
	}

	public string? currentSong()
	{
		return audioWrapper.currentSong();
	}
	// returns with playback enabled
	public void selectSong(string? name)
	{
		if (name is null)
		{
			audioWrapper.reset();
		}
		else
		{
			audioWrapper.selectSong(name);
		}
	}
	public TimeSpan currentTime()
	{
		return audioWrapper.currentTime();
	}
	public void pause()
	{
		audioWrapper.pause();
		//if (audioWrapper.pause() is true)
		//	OnPlaybackChange(PlaybackState.Paused);
	}
	public void play()
	{
		audioWrapper.play();
		//if (audioWrapper.play() is true)
		//	OnPlaybackChange(PlaybackState.Playing);
	}
	public PlaybackState playbackState()
	{
		return audioWrapper.playbackState();
	}
	public float currentVolume()
	{
		return audioWrapper.currentVolume();
	}
	public void addVolume(float delta)
	{
		if (audioWrapper.addVolume(delta) is true)
			OnVolumeChange(audioWrapper.currentVolume());
	}

	public void setVolume(float vol)
	{
		if (audioWrapper.setVolume(vol) is true)
			OnVolumeChange(audioWrapper.currentVolume());
	}
	public void setTime(TimeSpan time)
	{
		if (audioWrapper.setTime(time) is true)
			OnTimeChange(audioWrapper.currentTime());
	}

	public void addTime(float delta)
	{
		if (audioWrapper.addTime(delta) is true)
			OnTimeChange(audioWrapper.currentTime());
	}
}
