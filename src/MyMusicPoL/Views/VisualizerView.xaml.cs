﻿using MusicBackend.Model;
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
			timer.Interval = TimeSpan.FromMilliseconds(10);
			timer.Tick += OnTimerTick;
			timer.Start();
		}

		SKColor spectrumColor = SKColors.White;
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
			float circleBump = 160+100*(float)power;

			var width = e.Info.Width;
			var height = e.Info.Height;
			CalculateNewColor((float)power);

			var fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = spectrumColor,
			};

			fillPaint.IsAntialias = true;
			canvas.Save();
			const float rotationAngle = 0.5F;
			const float endAngle = 180F*2;
			const float amplify = 400F;
			canvas.RotateDegrees(-180-rotationAngle,width / 2, height / 2);
			for (int i = 0; i < endAngle && i < processedBuffer.Length; ++i)
			{
				canvas.RotateDegrees(rotationAngle,width / 2, height / 2);
				canvas.DrawRoundRect(
					width / 2,
					height / 2 + circleBump - 1, 
					3,
					amplify * (float)processedBuffer[i],
					10,10,
					fillPaint);
			}
			canvas.RotateDegrees(180+rotationAngle,width / 2, height / 2);
			for (int i = 0; i < endAngle && i < processedBuffer.Length; ++i)
			{
				canvas.RotateDegrees(-rotationAngle,width / 2, height / 2);
				canvas.DrawRoundRect(
					width / 2,
					height / 2 + circleBump - 1, 
					3,
					amplify * (float)processedBuffer[i],
					10,10,
					fillPaint);
			}
			canvas.Restore();

			var circlePaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				IsAntialias = true,
			};
			if (circleImage is null)
			{
				circlePaint.Shader = SKShader.CreateRadialGradient(
					new SKPoint(width / 2, height / 2),
					circleBump,
					[ SKColors.White, spectrumColor ],
					[ 0.0F, 1.0F ],
					SKShaderTileMode.Clamp);
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

		static float Lerp(float a, float b, float t) => a + (b - a) * t;

		void CalculateNewColor(float value)
		{
			// create new color as time progresses
			var hue = Lerp(200f,344f,Math.Clamp(value,0f,1f));
			//var hue = Math.Clamp(50 * value + 250, 274, 352);
			spectrumColor = SKColor.FromHsv(hue,62,97);
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
