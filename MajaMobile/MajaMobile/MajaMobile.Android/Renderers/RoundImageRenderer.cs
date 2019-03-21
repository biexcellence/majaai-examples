using Android.Content;
using Android.Graphics;
using System;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(MajaMobile.Controls.RoundImage), typeof(MajaMobile.Droid.Renderers.RoundImageRenderer))]
namespace MajaMobile.Droid.Renderers
{
    class RoundImageRenderer : Xamarin.Forms.Platform.Android.ImageRenderer
    {

        public RoundImageRenderer(Context context) : base(context) { }

        protected override bool DrawChild(Canvas canvas, Android.Views.View child, long drawingTime)
        {
            try
            {
                var radius = Math.Min(Width, Height) / 2;
                var strokeWidth = 10;
                radius -= strokeWidth / 2;

                //Create path to clip
                using (var path = new Path())
                {

                    path.AddCircle(Width / 2, Height / 2, radius, Path.Direction.Ccw);
                    //canvas.Save();
                    canvas.ClipPath(path);

                    var result = base.DrawChild(canvas, child, drawingTime);

                    //canvas.Restore();

                    //// Create path for circle border
                    //path = new Path();
                    //path.AddCircle(Width / 2, Height / 2, radius, Path.Direction.Ccw);

                    //var paint = new Paint();
                    //paint.AntiAlias = true;
                    //paint.StrokeWidth = 5;
                    //paint.SetStyle(Paint.Style.Stroke);
                    //paint.Color = global::Android.Graphics.Color.White;

                    //canvas.DrawPath(path, paint);

                    //Properly dispose
                    //paint.Dispose();
                    return result;
                }
            }
            catch { }
            return base.DrawChild(canvas, child, drawingTime);
        }

    }
}