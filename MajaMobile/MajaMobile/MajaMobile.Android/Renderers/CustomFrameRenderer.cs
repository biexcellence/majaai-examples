using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using MajaMobile.Droid.Renderers;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//https://github.com/xamarin/Xamarin.Forms/issues/2423

[assembly: ExportRenderer(typeof(Frame), typeof(CustomFrameRenderer))]
namespace MajaMobile.Droid.Renderers
{
    public class CustomFrameRenderer : FrameRenderer
    {
        public CustomFrameRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (Element == null)
            {
                return;
            }

            UpdateBackground();
            UpdateElevation();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (string.Equals(e.PropertyName, nameof(Frame.BackgroundColor)))
            {
                UpdateBackground();
            }
            else if (string.Equals(e.PropertyName,nameof(Frame.HasShadow)))
            {
                UpdateElevation();
            }
        }

        private void UpdateBackground()
        {
            int[] colors = { Element.BackgroundColor.ToAndroid(), Element.BackgroundColor.ToAndroid() };
            var gradientDrawable = new GradientDrawable(GradientDrawable.Orientation.LeftRight, colors);
            gradientDrawable.SetCornerRadius(Element.CornerRadius * 2); // CornerRadius = HeightRequest in my case

            this.SetBackground(gradientDrawable);
        }

        private void UpdateElevation()
        {
            if (Build.VERSION.SdkInt >= (BuildVersionCodes)21)
                Elevation = Element.HasShadow ? 20 : 0;
        }
    }
}