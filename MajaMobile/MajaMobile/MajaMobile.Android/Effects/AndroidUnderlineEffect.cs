using System;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using System.ComponentModel;
using Xamarin.Forms;

[assembly: ResolutionGroupName("MajaMobile")]
[assembly: ExportEffect(typeof(MajaMobile.Droid.Effects.AndroidUnderlineEffect), "UnderlineEffect")]
namespace MajaMobile.Droid.Effects
{
    class AndroidUnderlineEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            SetUnderline(true);
        }

        protected override void OnDetached()
        {
            SetUnderline(false);
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);
            if (args.PropertyName == Label.TextProperty.PropertyName || args.PropertyName == Label.FormattedTextProperty.PropertyName)
            {
                SetUnderline(true);
            }

        }

        private void SetUnderline(bool underlined)
        {
            try
            {
                var textView = (TextView)Control;
                if (underlined)
                {
                    textView.PaintFlags |= PaintFlags.UnderlineText;
                }
                else
                {
                    textView.PaintFlags &= ~PaintFlags.UnderlineText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot underline Label. Error: ", ex.Message);
            }
        }

    }
}