using BiExcellence.OpenBi.Api.Commands.MajaAi;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    /// <summary>
    /// https://openweathermap.org/weather-conditions
    /// </summary>
    public class WeatherSymbol : SKCanvasView
    {
        public static readonly BindableProperty WeatherProperty = BindableProperty.Create(nameof(Weather), typeof(WeatherDetails), typeof(WeatherSymbol));

        public WeatherDetails Weather
        {
            get { return (WeatherDetails)GetValue(WeatherProperty); }
            set { SetValue(WeatherProperty, value); }
        }

        private int _symbolRendered =-1;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == WeatherProperty.PropertyName && _symbolRendered != Weather.SymbolNumber)
            {
                InvalidateSurface();
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var height = e.Info.Height;
            var width = height;
            var canvas = e.Surface.Canvas;
            canvas.Clear();
            
            switch (Weather.SymbolNumber)
            {
                case int s when s >= 200 && s < 300:
                    DrawThunderstorm(canvas, height, width);
                    break;
                case int s when s >= 300 && s < 400:
                    DrawShowerRain(canvas, height, width);
                    break;
                case int s when s >= 500 && s <= 504:
                    DrawRain(canvas, height, width);
                    break;
                case 511:
                    DrawSnow(canvas, height, width);
                    break;
                case int s when s >= 520 && s < 600:
                    DrawShowerRain(canvas, height, width);
                    break;
                case int s when s >= 600 && s < 700:
                    DrawSnow(canvas, height, width);
                    break;
                case int s when s >= 700 && s < 800:
                    DrawBrokenClouds(canvas, height, width);
                    //TODO: DrawMist
                    break;
                case 800:
                    DrawClearSky(canvas, height, width);
                    break;
                case 801:
                    DrawFewClouds(canvas, height, width);
                    break;
                case 802:
                    DrawScatteredClouds(canvas, height, width);
                    break;
                case 803:
                case 804:
                    DrawBrokenClouds(canvas, height, width);
                    break;
            }
            _symbolRendered = Weather.SymbolNumber;
        }

        private void DrawPath(SKCanvas canvas, SKPath path, SKRect bounds, int height, int width, SKPaint paint, float scale = 1, float xTranslateFactor = 1, float yTranslateFactor = 1)
        {
            canvas.Save();

            canvas.Translate(width / 2, height / 2);

            var widthRatio = width / bounds.Width;
            var heightRatio = height / bounds.Height;

            var ratio = Math.Min(widthRatio, heightRatio) * scale;

            canvas.Scale(ratio);

            canvas.Translate(-bounds.MidX * xTranslateFactor, -bounds.MidY * yTranslateFactor);

            canvas.DrawPath(path, paint);

            canvas.Restore();
        }

        //private void DrawBorder(SKCanvas canvas, int height, int width)
        //{
        //    using (var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black, Style = SKPaintStyle.Stroke })
        //    {
        //        canvas.DrawRect(0, 0, width, height, paint);
        //    }
        //}

        private void DrawClearSky(SKCanvas canvas, int height, int width, float factor = 1, float xFactor = 1, float yFactor = 1)
        {
            using (var paint = new SKPaint { IsAntialias = true })
            {
                if (!Weather.IsNightTime)
                {
                    paint.Shader = SKShader.CreateLinearGradient(
                                            new SKPoint(0f, 0f),
                                            new SKPoint(0f, height),
                                            new SKColor[] { SKColors.LightGoldenrodYellow, SKColors.Yellow, SKColors.Orange },
                                            new float[] { 0, 0.4f, 1 },
                                            SKShaderTileMode.Clamp);
                    canvas.DrawCircle((width / 2) * xFactor, (height / 2) * yFactor, width * 0.4f * factor, paint);
                }
                else
                {
                    //TODO:NIGHT
                    paint.Shader = SKShader.CreateLinearGradient(
                                     new SKPoint(0f, 0f),
                                     new SKPoint(0f, height),
                                     new SKColor[] { SKColors.LightGoldenrodYellow, SKColors.Yellow, SKColors.Orange },
                                     new float[] { 0, 0.4f, 1 },
                                     SKShaderTileMode.Clamp);
                    canvas.DrawCircle((width / 2) * xFactor, (height / 2) * yFactor, width * 0.4f * factor, paint);
                }
            }
        }

        private SKRect DrawScatteredClouds(SKCanvas canvas, int height, int width, float scale = 1, float xTranslateFactor = 1, float yTranslateFactor = 1)
        {
            using (var paint = new SKPaint { IsAntialias = true })
            {
                var path = SKPath.ParseSvgPathData(
                                  "M 180,170 H 25" +
                                  "A 35,35 0 0 1 60,92" +
                                  "A 40,35 0 0 1 155,90" +
                                  "A 40,35 0 0 1 180,170");
                SKRect bounds;
                path.GetTightBounds(out bounds);
                paint.Shader = SKShader.CreateLinearGradient(
                                          new SKPoint(0f, bounds.Top), new SKPoint(0f, bounds.Bottom),
                                          new SKColor[] { SKColors.White, SKColors.WhiteSmoke, SKColors.LightGray, SKColors.White },
                                          new float[] { 0, 0.5f, 0.7f, 1 },
                                          SKShaderTileMode.Clamp);
                DrawPath(canvas, path, bounds, height, width, paint, scale, xTranslateFactor, yTranslateFactor);
                return bounds;
            }
        }

        private SKRect DrawFewClouds(SKCanvas canvas, int height, int width)
        {
            DrawClearSky(canvas, height, width, 0.6f, 1.5f, 0.9f);
            return DrawScatteredClouds(canvas, height, width, 0.8f, 1.25f, 0.9f);
        }

        private SKRect DrawBrokenClouds(SKCanvas canvas, int height, int width)
        {
            using (var paint = new SKPaint { IsAntialias = true })
            {
                var path = SKPath.ParseSvgPathData(
                                  "M 180,170 H 25" +
                                  "A 35,35 0 0 1 60,92" +
                                  "A 40,35 0 0 1 155,90" +
                                  "A 40,35 0 0 1 180,170");
                SKRect bounds;
                path.GetTightBounds(out bounds);
                paint.Shader = SKShader.CreateLinearGradient(
                                         new SKPoint(0f, bounds.Top), new SKPoint(0f, bounds.Bottom),
                                         new SKColor[] { SKColors.LightGray, SKColors.Gray, SKColors.DarkGray },
                                         new float[] { 0, 0.7f, 1 },
                                         SKShaderTileMode.Clamp);
                DrawPath(canvas, path, bounds, height, width, paint, 0.8f, 0.8f, 1.1f);
            }
            return DrawScatteredClouds(canvas, height, width, 0.8f, 1.25f, 0.9f);
        }

        private void DrawRaindrop(SKCanvas canvas, int height, int width, SKRect bounds, float scale = 1, float xTranslateFactor = 1, float yTranslateFactor = 1)
        {
            using (var paint = new SKPaint { IsAntialias = true })
            {
                var path = SKPath.ParseSvgPathData(
                                  "M 90,100 L 95,85 L 100,100" +
                                  "A 6,10 0 1 1 90,100" +
                                  "M 97,112 A 155,45 0 0 0 99,105");
                paint.Shader = SKShader.CreateLinearGradient(
                                         new SKPoint(0f, bounds.Top), new SKPoint(0f, bounds.Bottom),
                                         new SKColor[] { SKColors.DodgerBlue, SKColors.LightBlue, SKColors.LightBlue, SKColors.DodgerBlue },
                                         new float[] { 0, 0.7f, 0.95f, 1 },
                                         SKShaderTileMode.Clamp);
                DrawPath(canvas, path, bounds, height, width, paint, scale, xTranslateFactor, yTranslateFactor);
            }
        }

        private void DrawRaindrops(SKCanvas canvas, int height, int width, SKRect bounds)
        {
            DrawRaindrop(canvas, height, width, bounds, 1.1f, 1.5f, 0.3f);
            DrawRaindrop(canvas, height, width, bounds, 1.7f, 1.1f, 0.7f);
            DrawRaindrop(canvas, height, width, bounds, 1.1f, 0.9f, 0.8f);
            DrawRaindrop(canvas, height, width, bounds, 1.1f, 0.7f, 0.4f);
        }

        private void DrawRain(SKCanvas canvas, int height, int width)
        {
            var bounds = DrawFewClouds(canvas, height, width);
            DrawRaindrops(canvas, height, width, bounds);
        }

        private void DrawShowerRain(SKCanvas canvas, int height, int width)
        {
            var bounds = DrawBrokenClouds(canvas, height, width);
            DrawRaindrops(canvas, height, width, bounds);
        }

        private void DrawThunderBolt(SKCanvas canvas, int height, int width, SKRect bounds, float scale = 1, float xTranslateFactor = 1, float yTranslateFactor = 1)
        {
            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = SKColors.Red })
            {
                var path = SKPath.ParseSvgPathData("M 5,0L 30,0 0,50 20,50 -10,90 3,58 -20,58");
                DrawPath(canvas, path, bounds, height, width, paint, scale, xTranslateFactor, yTranslateFactor);
            }
        }

        private void DrawThunderstorm(SKCanvas canvas, int height, int width)
        {
            var bounds = DrawBrokenClouds(canvas, height, width);
            DrawThunderBolt(canvas, height, width, bounds, 1, 0.5f, -0.1f);
            DrawThunderBolt(canvas, height, width, bounds, 0.66f, 0.1f, -0.4f);
        }

        private void DrawSnowflake(SKCanvas canvas, int height, int width, SKRect bounds, float scale = 1, float xTranslateFactor = 1, float yTranslateFactor = 1)
        {
            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.SteelBlue, StrokeWidth = 1 })
            {
                var path = SKPath.ParseSvgPathData("M 100,90 L 100,110 M 96,92 L 100,95 104,92 M 96,108 L 100,105 104,108" +
                                                   "M 108.7, 95 L 91.3, 105 M 104.9, 92.5 L 104.3, 97.5 108.9, 99.5 M 91.1, 100.5 L 95.7, 102.5 95.1, 107.5" +
                                                   "M 108.7, 105 L 91.3, 95 M 108.9, 100.5 L 104.3, 102.5 104.9, 107.5 M 95.1, 92.5 L 95.7, 97.5 91.1, 99.5");
                DrawPath(canvas, path, bounds, height, width, paint, scale, xTranslateFactor, yTranslateFactor);
            }
        }

        private void DrawSnow(SKCanvas canvas, int height, int width)
        {
            var bounds = DrawBrokenClouds(canvas, height, width);
            DrawSnowflake(canvas, height, width, bounds, 1.3f, 1.5f, 0.4f);
            DrawSnowflake(canvas, height, width, bounds, 2.5f, 1.1f, 0.8f);
            DrawSnowflake(canvas, height, width, bounds, 1.1f, 0.9f, 0.8f);
            DrawSnowflake(canvas, height, width, bounds, 1.7f, 0.7f, 0.6f);
        }

    }
}