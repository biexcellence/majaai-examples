using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

//[assembly: ExportRenderer(typeof(Frame), typeof(MajaMobile.iOS.Renderers.CustomFrameRenderer))]
namespace MajaMobile.iOS.Renderers
{
    //Ignores CornerRadius : https://forums.xamarin.com/discussion/140118/shadow-customrenderer-for-ios-dismisses-cornerradius
    //Because content of frame ignores frame bounds: https://github.com/xamarin/Xamarin.Forms/issues/2405
    public class CustomFrameRenderer : FrameRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null && e.NewElement.HasShadow)
            {
                Layer.CornerRadius = Element.CornerRadius;
                Layer.ShadowRadius = 2.0f;
                Layer.ShadowColor = UIColor.LightGray.CGColor;
                Layer.ShadowOffset = new CGSize(2, 2);
                Layer.ShadowOpacity = 0.80f;
                Layer.MasksToBounds = false;
            }
        }
    }
}