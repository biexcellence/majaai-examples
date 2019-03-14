﻿using Android.Graphics;
using Android.Widget;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(MajaMobile.Droid.Effects.AndroidStrikeThroughEffect), "StrikeThroughEffect")]
namespace MajaMobile.Droid.Effects
{
    class AndroidStrikeThroughEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            RenderEffect(true);
        }

        protected override void OnDetached()
        {
            RenderEffect(false);
        }

        protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if (args.PropertyName == Label.TextProperty.PropertyName || args.PropertyName == Label.FormattedTextProperty.PropertyName)
            {
                RenderEffect(true);
            }
        }

        private void RenderEffect(bool shouldApply)
        {
            try
            {
                var textView = (TextView)Control;
                if (shouldApply)
                {
                    textView.PaintFlags |= PaintFlags.StrikeThruText;
                }
                else
                {
                    textView.PaintFlags &= ~PaintFlags.StrikeThruText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot strike-through Label. Error: ", ex.Message);
            }
        }
    }
}