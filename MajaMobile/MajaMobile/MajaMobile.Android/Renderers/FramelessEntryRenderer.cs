using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MajaMobile.Controls.FramelessEntry), typeof(MajaMobile.Droid.Renderers.FramelessEntryRenderer))]
namespace MajaMobile.Droid.Renderers
{
    public class FramelessEntryRenderer : EntryRenderer
    {
        public FramelessEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                //https://github.com/facebook/react-native/issues/6096
                var hor = (Control.PaddingLeft + Control.PaddingRight) / 2;
                var ver = (Control.PaddingTop + Control.PaddingBottom) / 2;
                Control.SetPadding(hor, ver, hor, ver);
                
                Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
                //var gd = new GradientDrawable();
                //gd.SetColor(Android.Graphics.Color.Transparent);
                //Control.SetBackground(gd);
            }
        }
    }
}