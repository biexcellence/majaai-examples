using MajaMobile.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public enum ChatButtonDisplayMode
    {
        Microphone,
        Send,
        Listening,
        Undefined = -99,
    }

    public class ChatButton : SKCanvasView
    {
        public static readonly BindableProperty DisplayModeProperty = BindableProperty.Create(nameof(DisplayMode), typeof(ChatButtonDisplayMode), typeof(ChatButton), defaultValue: ChatButtonDisplayMode.Microphone);

        private ChatButtonDisplayMode _currentDisplayMode = ChatButtonDisplayMode.Undefined;
        public ChatButtonDisplayMode DisplayMode
        {
            get { return (ChatButtonDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == DisplayModeProperty.PropertyName && _currentDisplayMode != DisplayMode)
            {
                InvalidateSurface();
            }
        }

        public ChatButton()
        {

        }

        protected override async void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            var currentDisplayMode = _currentDisplayMode;
            _currentDisplayMode = DisplayMode;

            var height = e.Info.Height;
            var width = e.Info.Width;
            var canvas = e.Surface.Canvas;

            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ColorScheme.ChatButtonBackground.ToSKColor(), StrokeWidth = 0 })
            {

                switch (DisplayMode)
                {
                    case ChatButtonDisplayMode.Listening:
                        await this.ScaleTo(2, 250, Easing.SpringOut);
                        break;
                    case ChatButtonDisplayMode.Send:
                        canvas.Clear();

                        canvas.DrawCircle(width / 2, height / 2, width / 2, paint);

                        paint.Style = SKPaintStyle.StrokeAndFill;
                        paint.Color = ColorScheme.ChatButtonForeground.ToSKColor();

                        using (var path = new SKPath())
                        {
                            path.MoveTo(width * 0.785f, height * 0.5f);
                            path.LineTo(width * 0.33f, height * 0.27f);
                            path.LineTo(width * 0.33f, height * 0.46f);
                            path.LineTo(width * 0.523f, height * 0.5f);
                            path.LineTo(width * 0.33f, height * 0.54f);
                            path.LineTo(width * 0.33f, height * 0.73f);
                            path.Close();
                            canvas.DrawPath(path, paint);
                        }
                        break;
                    default:

                        if (currentDisplayMode == ChatButtonDisplayMode.Listening)
                        {
                            await this.ScaleTo(1, 250, Easing.SpringOut);
                        }
                        else
                        {

                            canvas.Clear();
                            canvas.DrawCircle(width / 2, height / 2, width / 2, paint);

                            using (var paint2 = new SKPaint { Style = SKPaintStyle.Stroke, Color = ColorScheme.ChatButtonForeground.ToSKColor(), StrokeWidth = width * 0.135f, StrokeCap = SKStrokeCap.Round })
                            {
                                canvas.DrawLine(width * 0.5f, height * 0.34f, width * 0.5f, height * 0.51f, paint2);

                                paint2.StrokeWidth = paint2.StrokeWidth * 0.33f;

                                using (var path = new SKPath())
                                {
                                    path.MoveTo(width * 0.35f, height * 0.5f);
                                    path.AddArc(new SKRect(width * 0.35f, height * 0.4f, width * 0.65f, height * 0.65f), 0, 180);
                                    path.MoveTo(width * 0.5f, height * 0.65f);
                                    path.LineTo(width * 0.5f, height * 0.735f);
                                    canvas.DrawPath(path, paint2);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}