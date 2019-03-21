using CoreGraphics;
using MajaMobile.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(FramelessEntry), typeof(MajaMobile.iOS.Renderers.FramelessEntryRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class FramelessEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.LeftView = new UIView(new CGRect(0, 0, 8, Control.Frame.Height));
                Control.RightView = new UIView(new CGRect(0, 0, 8, Control.Frame.Height));
                Control.LeftViewMode = UITextFieldViewMode.Always;
                Control.RightViewMode = UITextFieldViewMode.Always;

                Control.BorderStyle = UITextBorderStyle.None;
            }
            if (Element != null)
                Element.HeightRequest = 50;
        }
    }
}