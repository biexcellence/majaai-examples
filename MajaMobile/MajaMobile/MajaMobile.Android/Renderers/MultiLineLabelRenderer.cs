using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using MajaMobile.Controls;

[assembly: ExportRenderer(typeof(MultiLineLabel), typeof(MajaMobile.Droid.Renderers.MultiLineLabelRenderer))]
namespace MajaMobile.Droid.Renderers
{
    public class MultiLineLabelRenderer : LabelRenderer
    {
        public MultiLineLabelRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            var lines = ((MultiLineLabel)Element).Lines;
            Control.SetLines(lines);
            Control.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
        }
    }
}