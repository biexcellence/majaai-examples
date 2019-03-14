using Foundation;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(MajaMobile.iOS.Effects.StrikeThroughEffect), "StrikeThroughEffect")]
namespace MajaMobile.iOS.Effects
{
    class StrikeThroughEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            SetUnderline(true);
        }

        protected override void OnDetached()
        {
            SetUnderline(false);
        }

        protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == Label.TextProperty.PropertyName ||
                args.PropertyName == Label.FormattedTextProperty.PropertyName)
            {
                SetUnderline(true);
            }
        }

        private void SetUnderline(bool underlined)
        {
            try
            {
                var label = (UILabel)Control;
                if (label != null)
                {
                    var text = (NSMutableAttributedString)label.AttributedText;
                    if (text != null)
                    {
                        var range = new NSRange(0, text.Length);
                        if (underlined)
                        {
                            text.AddAttribute(UIStringAttributeKey.StrikethroughStyle,
                                NSNumber.FromInt32((int)NSUnderlineStyle.Single), range);
                        }
                        else
                        {
                            text.RemoveAttribute(UIStringAttributeKey.StrikethroughStyle, range);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot strike-through Label. Error: ", ex.Message);
            }
        }
    }
}