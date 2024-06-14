using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MusicBackend.Model;
using SkiaSharp;

namespace mymusicpol.Views
{
    /// <summary>
    /// Interaction logic for VisualizerView.xaml
    /// </summary>
    public partial class VisualizerView : Window
    {
        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            int[] attrValue,
            int attrSize
        );

        // change window topbar to dark theme
        protected override void OnSourceInitialized(EventArgs e)
        {
            if (
                DwmSetWindowAttribute(
                    new WindowInteropHelper(this).Handle,
                    19,
                    new[] { 1 },
                    4
                ) != 0
            )
                DwmSetWindowAttribute(
                    new WindowInteropHelper(this).Handle,
                    20,
                    new[] { 1 },
                    4
                );
        }

        public VisualizerView()
        {
            InitializeComponent();
            QueueModel.Instance.OnSongChange += OnSongChanged;
            PlayerModel.Instance.OnSongChange += OnSongChanged;
            CreateCircleImage(QueueModel.Instance.CurrentSong());
            CreateBgImage();
            visualizer = new();
            CompositionTarget.Rendering += OnUpdate;
        }

        SKColor spectrumColor = SKColors.White;
        Visualizer visualizer;
        SKMatrix scaleMatrix;
        SKBitmap? circleImage;
        SKImage bgImage;
        SKShader circleShader;
        string songTitle = "";

        private void OnUpdate(object? s, EventArgs e)
        {
            Canvas.InvalidateVisual();
        }

        private void canvas_PaintSurface(
            object sender,
            SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e
        )
        {
            DrawVisualizer(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        float SmoothStep(float a, float b, float val)
        {
            val = Math.Clamp((val - a) / (b - a), 0, 1);
            return val * val * (3 - 2 * val);
        }

        void DrawBg(SKCanvas canvas, int width, int height, float bump)
        {
            canvas.Save();
            var shift = SmoothStep(0F, 1F, bump);
            canvas.Scale(1F + shift, 1F + shift, width / 2, height / 2);
            //canvas.DrawBitmap(bgImage, new SKRect(0, 0, width, height));
            canvas.DrawImage(bgImage, new SKRect(0, 0, width, height));
            canvas.Restore();
        }

        private void DrawVisualizer(SKCanvas canvas, int width, int height)
        {
            canvas.Clear();

            var (processedBuffer, power) = visualizer.Update();
            float circleBump = 160 + 100 * (float)power;

            CalculateNewColor((float)power);

            DrawBg(canvas, width, height, (float)power);

            var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = spectrumColor,
                IsAntialias = true,
            };

            canvas.Save();
            const float rotationAngle = 0.5F;
            const float endAngle = 180F * 2;
            const float amplify = 400F;
            canvas.RotateDegrees(-180 - rotationAngle, width / 2, height / 2);
            for (int i = 0; i < endAngle && i < processedBuffer.Length; ++i)
            {
                canvas.RotateDegrees(rotationAngle, width / 2, height / 2);
                canvas.DrawRoundRect(
                    width / 2,
                    height / 2 + circleBump - 1,
                    3,
                    amplify * (float)processedBuffer[i],
                    10,
                    10,
                    fillPaint
                );
            }
            canvas.RotateDegrees(180 + rotationAngle, width / 2, height / 2);
            for (int i = 0; i < endAngle && i < processedBuffer.Length; ++i)
            {
                canvas.RotateDegrees(-rotationAngle, width / 2, height / 2);
                canvas.DrawRoundRect(
                    width / 2,
                    height / 2 + circleBump - 1,
                    3,
                    amplify * (float)processedBuffer[i],
                    10,
                    10,
                    fillPaint
                );
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
                    [SKColors.White, spectrumColor],
                    [0.0F, 1.0F],
                    SKShaderTileMode.Clamp
                );
            }
            else
            {
                // create matrix with translation to the middle
                scaleMatrix = SKMatrix.CreateScaleTranslation(
                    circleBump / 100,
                    circleBump / 100,
                    width / 2 - circleBump,
                    height / 2 - circleBump
                );
                circlePaint.Shader = circleShader.WithLocalMatrix(scaleMatrix);
            }
            // draw circle in the middle with circleBump width
            var circleRect = new SKRect(
                width / 2 - circleBump,
                height / 2 - circleBump,
                width / 2 + circleBump,
                height / 2 + circleBump
            );
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
            var hue = Lerp(200f, 280F, Math.Clamp(value, 0f, 1f));
            spectrumColor = SKColor.FromHsv(hue, 62, 97);
        }

        void CreateBgImage()
        {
            var assembly = GetType().Assembly;
            using var stream = assembly.GetManifestResourceStream(
                "mymusicpol.assets.background.jpg"
            );
            var bgBitmap = SKBitmap.Decode(stream);
            bgImage = SKImage.FromBitmap(bgBitmap);
        }

        void CreateCircleImage(Song? song)
        {
            if (song?.name is null)
            {
                this.songTitle = "";
            }
            else
            {
                this.songTitle = song.name;
            }

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

                using (
                    var data = new SKManagedStream(
                        new MemoryStream(song.Album.Cover)
                    )
                )
                {
                    var codec = SKCodec.Create(data);
                    info = new SKImageInfo(codec.Info.Width, codec.Info.Height);
                    codec.GetPixels(out bytes);
                }
                var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                image.InstallPixels(
                    info,
                    gcHandle.AddrOfPinnedObject(),
                    info.RowBytes,
                    delegate
                    {
                        gcHandle.Free();
                    }
                );
                image = image.Resize(
                    new SKImageInfo(200, 200),
                    SKFilterQuality.High
                );

                circleShader = SKShader.CreateBitmap(
                    image,
                    SKShaderTileMode.Clamp,
                    SKShaderTileMode.Clamp
                );
                circleImage = image;
            }
        }

        private void Window_Closing(
            object sender,
            System.ComponentModel.CancelEventArgs e
        )
        {
            CompositionTarget.Rendering -= OnUpdate;
            QueueModel.Instance.OnSongChange -= OnSongChanged;
            PlayerModel.Instance.OnSongChange -= OnSongChanged;
            visualizer.Dispose();
        }
    }
}
