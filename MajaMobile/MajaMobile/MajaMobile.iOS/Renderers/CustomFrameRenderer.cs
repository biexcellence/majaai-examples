using CoreGraphics;
using UIKit;
using Xamarin.Forms.Platform.iOS;

//[assembly: ExportRenderer(typeof(Frame), typeof(MajaMobile.iOS.Renderers.CustomFrameRenderer))]
namespace MajaMobile.iOS.Renderers
{
    //Ignores CornerRadius : https://forums.xamarin.com/discussion/140118/shadow-customrenderer-for-ios-dismisses-cornerradius
    //Because content of frame ignores frame bounds: https://github.com/xamarin/Xamarin.Forms/issues/2405
    public class CustomFrameRenderer : FrameRenderer
    {
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            // Update shadow to match better material design standards of elevation
            Layer.ShadowRadius = Element.CornerRadius;
            Layer.ShadowColor = UIColor.Gray.CGColor;
            Layer.ShadowOffset = new CGSize(2, 2);
            Layer.ShadowOpacity = 0.80f;
            Layer.ShadowPath = UIBezierPath.FromRect(Layer.Bounds).CGPath;
            Layer.MasksToBounds = false;


            //Layer.BorderColor = UIColor.White.CGColor;
            //Layer.CornerRadius = 10;
            //Layer.MasksToBounds = false;
            //Layer.ShadowOffset = new CGSize(-2, 2);
            //Layer.ShadowRadius = 5;
            //Layer.ShadowOpacity = 0.4f;
        }
    }
}