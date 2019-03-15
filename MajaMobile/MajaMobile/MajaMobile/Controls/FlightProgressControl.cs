using MajaMobile.Messages;
using MajaMobile.Models;
using MajaMobile.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class FlightProgressControl : SKCanvasView
    {

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            var message = BindingContext as MajaConversationMessageFlightStatus;
            if (message != null && message.FlightStatus != null)
            {
                var status = message.FlightStatus;
                var height = e.Info.Height;
                var width = e.Info.Width;
                var canvas = e.Surface.Canvas;
                canvas.Clear();
                var planeColor = status.ArrivalGateDelayMinutes > 15 ? ColorScheme.FlightColorRed.ToSKColor() : ColorScheme.FlightColorGreen.ToSKColor();
                var circleRad = height * 0.1f;
                var maxX = width - circleRad * 2;
                var minX = circleRad * 2;
                using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = planeColor, StrokeWidth = height * 0.07f })
                {
                    switch (status.Status)
                    {
                        case FlightStatusCodes.Active when status.GateDepartureUtc.HasValue && status.GateArrivalUtc.HasValue:
                            var progressX = (1.0f - (float)(status.GateArrivalUtc.Value.Subtract(status.DateTimeUtc).TotalMinutes / status.GateArrivalUtc.Value.Subtract(status.GateDepartureUtc.Value).TotalMinutes)) * (width - circleRad * 4);
                            progressX = Math.Min(Math.Max(minX, progressX), maxX);

                            canvas.DrawLine(circleRad, height / 2, progressX, height / 2, paint);
                            canvas.DrawCircle(circleRad, height / 2, circleRad, paint);

                            paint.Color = Color.LightGray.ToSKColor();
                            canvas.DrawLine(progressX, height / 2, width - circleRad, height / 2, paint);
                            canvas.DrawCircle(width - circleRad, height / 2, circleRad, paint);

                            paint.Color = planeColor;
                            DrawPlane(canvas, e.Info, progressX, paint, minX, maxX);
                            break;
                        case FlightStatusCodes.Canceled:
                        case FlightStatusCodes.Scheduled:
                            if(status.Status == FlightStatusCodes.Canceled)
                            {
                                planeColor = ColorScheme.FlightColorRed.ToSKColor();
                            }
                            else
                            {
                                if (status.DepartureGateDelayMinutes > 15)
                                {
                                    planeColor = ColorScheme.FlightColorRed.ToSKColor();
                                }
                            }
                            paint.Color = Color.LightGray.ToSKColor();
                            canvas.DrawLine(minX, height / 2, width - circleRad, height / 2, paint);
                            canvas.DrawCircle(width - circleRad, height / 2, circleRad, paint);

                            paint.Color = planeColor;
                            DrawPlane(canvas, e.Info, minX, paint, minX, maxX);
                            break;
                        case FlightStatusCodes.Landed:
                            canvas.DrawLine(circleRad, height / 2, maxX - paint.StrokeWidth, height / 2, paint);
                            canvas.DrawCircle(circleRad, height / 2, circleRad, paint);

                            paint.Color = planeColor;
                            DrawPlane(canvas, e.Info, maxX, paint, minX, maxX);
                            break;
                    }
                }
            }
        }

        private void DrawPlane(SKCanvas canvas, SKImageInfo info, float x, SKPaint paint, float minX, float maxX)
        {
            var height = info.Height;
            var width = info.Width;
            using (var path = SKPath.ParseSvgPathData("M21 16v-2l-8-5V3.5c0-.83-.67-1.5-1.5-1.5S10 2.67 10 3.5V9l-8 5v2l8-2.5V19l-2 1.5V22l3.5-1 3.5 1v-1.5L13 19v-5.5l8 2.5z"))
            {
                path.Transform(SKMatrix.MakeRotationDegrees(90));
                if (path.GetTightBounds(out var bounds))
                {
                    var scale = 2f / 3f / (bounds.Height / height);
                    path.Transform(SKMatrix.MakeScale(scale, scale));
                    if (path.GetTightBounds(out bounds))
                    {
                        var newX = x + bounds.Width / 2;
                        if ((newX - bounds.Width) <= minX)
                        {
                            newX = minX + bounds.Width;
                        }
                        else if (newX >= maxX)
                        {
                            newX = maxX;
                        }
                        path.Transform(SKMatrix.MakeTranslation(newX, (height / 2) - bounds.Height / 2 - (1 * scale / bounds.Height * height) - paint.StrokeWidth / 4));
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }
    }
}