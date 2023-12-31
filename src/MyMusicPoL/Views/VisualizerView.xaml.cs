using MusicBackend.Filters;
using MusicBackend.Interfaces;
using MusicBackend.Model;
using SkiaSharp;
using System.Windows;

namespace mymusicpol.Views
{
	/// <summary>
	/// Interaction logic for VisualizerView.xaml
	/// </summary>
	public partial class VisualizerView : Window, SampleObserver
	{
		public VisualizerView()
		{
			InitializeComponent();
			PlayerModel.Instance.SamplesAccumulated += SamplesNotify;
			//PlayerModel.Instance.Subscribe(this);
			//var timer = new DispatcherTimer();
			//timer.Interval = TimeSpan.FromMilliseconds(33);
			//timer.Tick += (s, e) => Canvas.InvalidateVisual();
			//timer.Start();
		}

		private int counter = 0;

		private double[]? sampleBuffer;
		private double[]? processedBuffer;
		private IFilter? fftFilter;
		private IFilter?[] filters;

		private void canvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
		{
			SKCanvas canvas = e.Surface.Canvas;
			canvas.Clear();

			var fillPaint = new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Color = SKColors.HotPink
			};

			//fillPaint.Shader = SKShader.CreateLinearGradient(
			//	new SKPoint(0, 0),
			//	new SKPoint(0, 20),
			//	new SKColor[] { SKColors.Blue, SKColors.Red },
			//	new float[] { 0, 1 },
			//	SKShaderTileMode.Clamp);


			if (sampleBuffer is not null)
			{
				var width = e.Info.Width;
				var height = e.Info.Height;
				processedBuffer = fftFilter.process(sampleBuffer);
				foreach (var filter in filters)
				{

					processedBuffer = filter.process(processedBuffer);

				}
				//processedBuffer = logScaleFilter.process(processedBuffer);
				//processedBuffer = binFilter.process(processedBuffer);

				// draw rectangle for each bin starting from bottom of screen
				// with width of 8 pixels
				for (int i = 0; i < processedBuffer.Length; ++i)
				{
					var x = i * 2;
					var y = (float)(height - height * processedBuffer[i]);
					var rect = new SKRect(x, y, x + 2, height);
					canvas.DrawRect(rect, fillPaint);
				}
			}

		}


		public void SamplesNotify(float[] samples)
		{
			if (sampleBuffer is null)
			{
				sampleBuffer = new double[samples.Length];
				fftFilter = new FftFilter();
				filters = new IFilter[]
				{
					new LogScale(1.02,samples.Length/2),
					new LogBinFilter(1.02,1.5),
					new ClampBinFilter(0.10),
				};
				//fftFilter = new FftFilter();
				//logScaleFilter = new LogScale(1.02,samples.Length/2);
				//binFilter = new ClampBinFilter(10.0);
			}
			for (int i = 0; i != samples.Length; ++i)
			{
				sampleBuffer[i] = samples[i];
			}


			Application.Current.Dispatcher.Invoke(() => Canvas.InvalidateVisual());
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//PlayerModel.Instance.Unsubscribe(this);
			PlayerModel.Instance.SamplesAccumulated -= SamplesNotify;
		}
	}
}
