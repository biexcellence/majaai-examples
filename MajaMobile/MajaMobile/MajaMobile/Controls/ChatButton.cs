using MajaMobile.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Diagnostics;
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


        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _alpha;
        private readonly double _cycleTime = 1500;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == DisplayModeProperty.PropertyName && _currentDisplayMode != DisplayMode)
            {
                _currentDisplayMode = DisplayMode;
                if (DisplayMode == ChatButtonDisplayMode.Listening)
                {
                    StartListening();
                }
                else
                {
                    InvalidateSurface();
                }
            }
        }

        private void StartListening()
        {
            _alpha = 0;
            _stopwatch.Restart();
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                var oldAlpha = _alpha = (_stopwatch.Elapsed.TotalMilliseconds % _cycleTime) / _cycleTime;
                if (_alpha <= 1.0 / 3)
                {
                    _alpha = _alpha / (1.0 / 3);
                }
                else
                {
                    _alpha = 1 - ((_alpha - (1.0 / 3)) / (2.0 / 3));
                }
                InvalidateSurface();
                if (DisplayMode != ChatButtonDisplayMode.Listening)
                    _stopwatch.Reset();
                return DisplayMode == ChatButtonDisplayMode.Listening;
            });
        }

        public ChatButton()
        {

        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            var height = e.Info.Height;
            var width = e.Info.Width;
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ColorScheme.ChatButtonBackground.ToSKColor(), StrokeWidth = 0 })
            {
                canvas.DrawCircle(width / 2, height / 2, width / 2, paint);
                switch (DisplayMode)
                {
                    case ChatButtonDisplayMode.Listening:
                        paint.Color = Color.FromRgba(255.0, 255.0, 255.0, _alpha).ToSKColor();
                        canvas.DrawCircle(width / 2, height / 2, width * 0.4f, paint);
                        break;
                    case ChatButtonDisplayMode.Send:

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
                        break;
                }
            }
        }
    }
}