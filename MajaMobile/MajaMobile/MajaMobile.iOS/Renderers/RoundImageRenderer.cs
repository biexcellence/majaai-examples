﻿using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MajaMobile.Controls.RoundImage), typeof(MajaMobile.iOS.Renderers.RoundImageRenderer))]
namespace MajaMobile.iOS.Renderers
{
    class RoundImageRenderer : ImageRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            CreateCircle();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == VisualElement.HeightProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName)
            {
                CreateCircle();
            }
        }

        private void CreateCircle()
        {
            try
            {
                double min = Math.Min(Element.Width, Element.Height);
                Control.Layer.CornerRadius = (float)(min / 2.0);
                Control.Layer.MasksToBounds = false;
                //Control.Layer.BorderColor = Color.White.ToCGColor();
                Control.Layer.BorderWidth = 0;
                Control.ClipsToBounds = true;
            }
            catch { }
        }

    }
}