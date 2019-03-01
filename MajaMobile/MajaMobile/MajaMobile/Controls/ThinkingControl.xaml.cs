using MajaMobile.Messages;
using MajaMobile.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public partial class ThinkingControl : ContentView
    {
        MajaConversationMessageThinking _message;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _step;
        private int _currentDot;
        private readonly double _cycleTime = 2000;

        public ThinkingControl()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext is MajaConversationMessageThinking message)
            {
                _message = message;
                StartThinking();
            }
        }

        private void StartThinking()
        {
            _step = 0;
            _stopwatch.Restart();
            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                _step = (_stopwatch.Elapsed.TotalMilliseconds % _cycleTime) / (_cycleTime / 4);
                var oldDot = _currentDot;
                _currentDot = (int)Math.Floor(_step);
                if (_currentDot <= 2 || oldDot <= 2)
                {
                    Canvas.InvalidateSurface();
                }
                var disposed = _message.Disposed;
                if (disposed)
                    _stopwatch.Reset();
                return !disposed;
            });
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var height = e.Info.Height;
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear();
            if (_message != null)
            {
                using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ColorScheme.MajaMessageTextColor.ToSKColor(), StrokeWidth = 0 })
                {
                    for (int i = 0; i < 3; i++)
                    {
                        double radius = 10;
                        if (i == _currentDot)
                        {
                            var d = _step - _currentDot;
                            var step = d / 0.5;
                            if (step > 1)
                            {
                                step = 1 - (step - 1);
                            }
                            radius = step * radius + radius;
                        }
                        canvas.DrawCircle(20 + 40 * i, height / 2, (float)radius, paint);
                    }
                }
            }
        }
    }
}