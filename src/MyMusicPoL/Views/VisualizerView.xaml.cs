using MusicBackend.Filters;
using MusicBackend.Interfaces;
using MusicBackend.Model;
using SkiaSharp;
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
			//PlayerModel.Instance.SamplesAccumulated += SamplesNotify;
			//PlayerModel.Instance.Subscribe(this);
			visualizer = new();
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(15);
			timer.Tick += OnTimerTick;
			timer.Start();
		}


		private int counter = 0;
		Visualizer visualizer;
		DispatcherTimer timer;

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
			canvas.DrawText($"Frame: {counter++}", 0, 20, textPaint);

			// draw circle in the middle with circleBump width
			var circlePaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColors.Blue
			};
			var circleRect = new SKRect (
				width / 2 - circleBump,
				height / 2 - circleBump,
				width / 2 + circleBump,
				height / 2 + circleBump);
			canvas.DrawOval(circleRect, circlePaint);


			var fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColors.HotPink
			};


			// draw rectangle for each bin starting from bottom of screen
			// with width of 8 pixels

			fillPaint.IsAntialias = true;
			canvas.Save();
			for (int i = 0; i < 180; ++i)
			{
				canvas.RotateDegrees(2,width / 2, height / 2);
				canvas.DrawRoundRect(
					width / 2,
					height/2 + circleBump, 
					4,
					400*(float)processedBuffer[i],
					10,10,
					fillPaint);
				//var x = i * 2;
				//var y = (float)(height - height * processedBuffer[i]);
				//var rect = new SKRect(x, y, x + 2, height);
				//canvas.DrawRect(rect, fillPaint);
			}
			canvas.Restore();


			//canvas.Save();
			//// rotate canvas
			//canvas.RotateDegrees(45, width / 2, height / 2);
			//canvas.DrawRect(width / 2, height/2 + circleBump, 20, 60,fillPaint);
			//canvas.RotateDegrees(45, width / 2, height / 2);
			//canvas.DrawRect(width / 2, height/2 + circleBump, 20, 20,fillPaint);
			//canvas.Restore();

		}
		//public void SamplesNotify(float[] samples)
		//{
		//	if (sampleBuffer is null)
		//	{
		//		sampleBuffer = new double[samples.Length];
		//		fftFilter = new FftFilter();
		//		filters = new IFilter[]
		//		{
		//			new LogScale(1.02,samples.Length/2),
		//			new LogBinFilter(1.02,1.5),
		//			new ClampBinFilter(0.10),
		//		};
		//		//fftFilter = new FftFilter();
		//		//logScaleFilter = new LogScale(1.02,samples.Length/2);
		//		//binFilter = new ClampBinFilter(10.0);
		//	}
		//	for (int i = 0; i != samples.Length; ++i)
		//	{
		//		sampleBuffer[i] = samples[i];
		//	}


		//	Application.Current.Dispatcher.Invoke(() => Canvas.InvalidateVisual());
		//}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			timer.Stop();
			timer.Tick -= OnTimerTick;
			visualizer.Dispose();
		}
	}
}
