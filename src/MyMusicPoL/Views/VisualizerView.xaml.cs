using MusicBackend.Filters;
using MusicBackend.Interfaces;
using MusicBackend.Model;
using SkiaSharp;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace mymusicpol.Views
{
	/// <summary>
	/// Interaction logic for VisualizerView.xaml
	/// </summary>
	public partial class VisualizerView : Window
	{
		public VisualizerView()
		{
			InitializeComponent();
			QueueModel.Instance.OnSongChange += OnSongChanged;
			PlayerModel.Instance.OnSongChange += OnSongChanged;
			CreateCircleImage(QueueModel.Instance.currentSong());
			visualizer = new();
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(25);
			timer.Tick += OnTimerTick;
			timer.Start();
		}

		private int counter = 0;
		Visualizer visualizer;
		DispatcherTimer timer;
		SKMatrix scaleMatrix;
		SKBitmap? circleImage;
		SKShader circleShader;

		private void OnTimerTick(object? s,EventArgs e)
		{
			Canvas.InvalidateVisual();
		}

		private void canvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;
			canvas.Clear();

			var (processedBuffer,power) = visualizer.Update();
			float circleBump = 80+100*(float)power;

			var width = e.Info.Width;
			var height = e.Info.Height;

			// draw text in top left corner
			var textPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColors.White,
				TextSize = 20
			};

			var fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColors.HotPink
			};
			// draw rectangle for each bin starting from bottom of screen
			// with width of 8 pixels

			fillPaint.IsAntialias = true;
			canvas.Save();
			canvas.RotateDegrees(-180,width / 2, height / 2);
			for (int i = 0; i < 180*4 && i < processedBuffer.Length; ++i)
			{
				canvas.RotateDegrees(0.25F,width / 2, height / 2);
				canvas.DrawRoundRect(
					width / 2,
					height/2 + circleBump - 1, 
					3,
					400*(float)processedBuffer[i],
					10,10,
					fillPaint);
			}
			canvas.RotateDegrees(180,width / 2, height / 2);
			for (int i = 0; i < 180*4 && i < processedBuffer.Length; ++i)
			{
				canvas.RotateDegrees(-0.25F,width / 2, height / 2);
				canvas.DrawRoundRect(
					width / 2,
					height/2 + circleBump - 1, 
					3,
					400*(float)processedBuffer[i],
					10,10,
					fillPaint);
			}
			canvas.Restore();

			// Draw circle
			var circlePaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				IsAntialias = true,
			};
			if (circleImage is null)
			{
				circlePaint.Color = SKColors.HotPink;
			}
			else
			{
				// create matrix with translation to the middle
				scaleMatrix = SKMatrix.CreateScaleTranslation(
					circleBump / 100,
					circleBump / 100,
					width / 2 - circleBump,
					height / 2 - circleBump);
				circlePaint.Shader = circleShader.WithLocalMatrix(scaleMatrix);
			}
			// draw circle in the middle with circleBump width
			var circleRect = new SKRect (
				width / 2 - circleBump,
				height / 2 - circleBump,
				width / 2 + circleBump,
				height / 2 + circleBump);
			canvas.DrawOval(circleRect, circlePaint);
		}

		void OnSongChanged(Song song)
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				CreateCircleImage(song);
			});	
		}

		void CreateCircleImage(Song? song)
		{
			if (song?.Album.Cover is null)
			{
				this.circleImage = null;
				return;
			}
			else 
			{
				var image = new SKBitmap();

				byte[] bytes;
				SKImageInfo info;

				using (var data = new SKManagedStream(new MemoryStream(song.Album.Cover)))
				{
					var codec = SKCodec.Create(data);
					info = new SKImageInfo(codec.Info.Width, codec.Info.Height);
					codec.GetPixels(out bytes);
				}
				var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
				image.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); });
				image = image.Resize(new SKImageInfo(200, 200), SKFilterQuality.High);

				this.circleShader = SKShader.CreateBitmap(image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
				this.circleImage = image;
			}

		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			timer.Stop();
			timer.Tick -= OnTimerTick;
			QueueModel.Instance.OnSongChange -= OnSongChanged;
			PlayerModel.Instance.OnSongChange -= OnSongChanged;
			visualizer.Dispose();
		}
	}
}
