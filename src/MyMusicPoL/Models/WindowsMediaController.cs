using MusicBackend.Model;
using Windows.Media;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.Storage;
using System.IO;
namespace mymusicpol.Models;

internal class WindowsMediaController
{

	private readonly PlayerModel playerModel;
	private readonly QueueModel queueModel;
	//private readonly MediaPlayer mediaPlayer;
	private readonly SystemMediaTransportControls smtControls;
	public WindowsMediaController(
		PlayerModel playerModel, 
		QueueModel queueModel)
	{
		this.playerModel = playerModel;
		this.queueModel = queueModel;
		// GetForCurrentView doesnt work
		var winHandle = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
		smtControls = SystemMediaTransportControlsInterop.GetForWindow(winHandle);
		smtControls.IsPlayEnabled = true;
		smtControls.IsPauseEnabled = true;
		smtControls.IsNextEnabled = true;
		smtControls.IsPreviousEnabled = true;
		smtControls.IsFastForwardEnabled = true;
		smtControls.IsRewindEnabled = true;
		smtControls.ButtonPressed += OnButtonPressed;


		// attach handlers
		playerModel.OnSongChange += UpdateSong;
		playerModel.OnPlaybackChange += UpdatePlaybackStatus;

		// set initial state
		UpdatePlaybackStatus(playerModel.playbackState());
		if (playerModel.currentSong() is not null)
		{
			UpdateSong(Song.fromPath(playerModel.currentSong()));
		}
		else
		{
			UpdateSong(null);
		}
	}

	private void UpdatePlaybackStatus(PlaybackState state)
	{
		smtControls.PlaybackStatus = state switch
		{
			PlaybackState.Playing => MediaPlaybackStatus.Playing,
			PlaybackState.Paused => MediaPlaybackStatus.Paused,
			PlaybackState.Stopped => MediaPlaybackStatus.Stopped,
			_ => MediaPlaybackStatus.Closed,
		};
		smtControls.DisplayUpdater.Update();
	}

	public void UpdateSong(Song? song)
	{
		if (song is null)
		{
			smtControls.PlaybackStatus = MediaPlaybackStatus.Closed;
		}
		else
		{
			smtControls.PlaybackStatus = MediaPlaybackStatus.Playing;
			smtControls.DisplayUpdater.Type = MediaPlaybackType.Music;
			smtControls.DisplayUpdater.MusicProperties.Title = song.name;
			smtControls.DisplayUpdater.MusicProperties.Artist = song.artist;
			SetThumbnailFromAlbumCover(song.Album);
		}
		smtControls.DisplayUpdater.Update();
	}

	private void SetThumbnailFromAlbumCover(SongAlbum album)
	{
		var cover = album.Cover;
		if (cover is not null)
		{
			var stream = ByteToRandomAccessStream(cover);
			WriteStreamToFile(stream, "albumCover.png");
			smtControls.DisplayUpdater.Thumbnail = 
				RandomAccessStreamReference.CreateFromStream(stream);
		}
		else
		{
			smtControls.DisplayUpdater.Thumbnail =
				RandomAccessStreamReference.CreateFromUri(new Uri("pack://application:,,,/assets/default.png"));
		}
	}

	private static void WriteStreamToFile(IRandomAccessStream stream, string path)
	{
		using (var fileStream = new FileStream(path, FileMode.Create))
		{
			stream.AsStreamForRead().CopyTo(fileStream);
		}
	}

	private static IRandomAccessStream ByteToRandomAccessStream(byte[] bytes)
	{
		var stream = new InMemoryRandomAccessStream();
		using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
		{
			writer.WriteBytes(bytes);
			writer.StoreAsync().GetResults();
		}
		return stream;
	}

	private void OnButtonPressed(
		SystemMediaTransportControls sender, 
		SystemMediaTransportControlsButtonPressedEventArgs args)
	{
		Action? func;
		func = args.Button switch
		{
			SystemMediaTransportControlsButton.Play =>
				() => playerModel.play(),
			SystemMediaTransportControlsButton.Pause =>
				() => playerModel.pause(),
			SystemMediaTransportControlsButton.Next =>
				() => queueModel.forceNextSong(),
			SystemMediaTransportControlsButton.Previous =>
				() => queueModel.forcePrevSong(),
			SystemMediaTransportControlsButton.FastForward =>
				() => playerModel.addTime(10),
			SystemMediaTransportControlsButton.Rewind =>
				() => playerModel.addTime(-10),
			_ => null,
		};

		if (func is not null)
		{
			App.Current.Dispatcher.Invoke(func);
		}
	}
}
