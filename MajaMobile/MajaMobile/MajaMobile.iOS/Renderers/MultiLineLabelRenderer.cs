using MajaMobile.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MultiLineLabel), typeof(MajaMobile.iOS.Renderers.MultiLineLabelRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class MultiLineLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            if (Control != null && Element != null)
            {
                Control.Lines = ((MultiLineLabel)Element).Lines;
                Control.LineBreakMode = UIKit.UILineBreakMode.TailTruncation;
            }
        }
    }
}