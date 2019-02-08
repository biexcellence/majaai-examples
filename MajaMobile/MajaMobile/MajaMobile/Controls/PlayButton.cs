using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class AudioButton : SKCanvasView
    {
        public static readonly BindableProperty IsPlayingProperty = BindableProperty.Create(nameof(IsPlaying), typeof(bool), typeof(AudioButton), defaultValue: false);

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == IsPlayingProperty.PropertyName)
            {
                InvalidateSurface();
            }
        }

        public AudioButton()
        {
            //IgnorePixelScaling = true;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            var height = e.Info.Height;
            var width = e.Info.Width;
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.StrokeAndFill, Color = new SKColor(0, 0, 0), StrokeWidth = 0 })
            using (var path = new SKPath())
            {
                if (!IsPlaying)
                {
                    path.MoveTo(0, 0);
                    path.LineTo(width, height / 2);
                    path.LineTo(0, height);
                    path.Close();
                }
                else
                {
                    path.MoveTo(0, 0);
                    path.LineTo(width, 0);
                    path.LineTo(width, height);
                    path.LineTo(0, height);
                    path.Close();
                }
                canvas.DrawPath(path, paint);
            }
        }
    }
}