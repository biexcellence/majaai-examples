using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("MajaMobile")]
[assembly: ExportEffect(typeof(MajaMobile.iOS.Effects.UnderlineEffect), "UnderlineEffect")]
namespace MajaMobile.iOS.Effects
{
    class UnderlineEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var label = Control as UILabel;
            if (label != null && label.Text != null)
            {
                var attrString = new NSMutableAttributedString(label.Text);

                attrString.AddAttribute(UIStringAttributeKey.UnderlineStyle,
                                         NSNumber.FromInt32((int)NSUnderlineStyle.Single),
                                         new NSRange(0, attrString.Length));

                label.AttributedText = attrString;
            }
        }

        protected override void OnDetached()
        {

        }
    }
}