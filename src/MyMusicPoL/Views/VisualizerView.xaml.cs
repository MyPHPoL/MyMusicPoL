using MusicBackend.Model;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

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
            PlayerModel.Instance.Subscribe(this);
            //var timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(33);
            //timer.Tick += (s, e) => Canvas.InvalidateVisual();
            //timer.Start();
        }

        private int counter = 0;

        private float[]? sampleBuffer;

		private void canvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
		{
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();

            var fillPaint = new SKPaint
            {
				Style = SKPaintStyle.Fill,
				Color = SKColors.HotPink
			};


            if (sampleBuffer is not null)
            {
				var width = e.Info.Width;
				var height = e.Info.Height;
				var xScale = width / (float)sampleBuffer.Length;
				var yScale = height;
				var path = new SKPath();
				path.MoveTo(0, yScale * sampleBuffer[0]);
				for (int i = 1; i != sampleBuffer.Length; ++i)
                {
					path.LineTo(i * xScale, yScale * sampleBuffer[i]);
				}
				path.LineTo(width, height );
				path.LineTo(0, height);
				path.Close();
				canvas.DrawPath(path, fillPaint);
			}

		}


		public void SamplesNotify(float[] samples)
		{
            if (sampleBuffer is null)
            {
                sampleBuffer = new float[samples.Length]; 
            }
            for (int i = 0; i != samples.Length; ++i)
            {
                sampleBuffer[i] = samples[i];
			}

            Application.Current.Dispatcher.Invoke(() => Canvas.InvalidateVisual());
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            PlayerModel.Instance.Unsubscribe(this);
		}
	}
}
