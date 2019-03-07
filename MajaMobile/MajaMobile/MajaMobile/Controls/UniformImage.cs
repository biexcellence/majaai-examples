using System;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class UniformImage : Image
    {
        public static readonly BindableProperty AspectExProperty = BindableProperty.Create(nameof(AspectEx), typeof(AspectExt), typeof(UniformImage), AspectExt.AspectFit, propertyChanged: OnAspectExPropertyChanged);

        static void OnAspectExPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var aspectEx = (AspectExt)newValue;
            Aspect aspect;
            if (Enum.TryParse(aspectEx.ToString(), out aspect))
            {
                (bindable as UniformImage).Aspect = aspect;
            }
        }

        public AspectExt AspectEx
        {
            get { return (AspectExt)GetValue(AspectExProperty); }
            set { SetValue(AspectExProperty, value); }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            // the returned sizeRequest contains the dimensions of the image
            SizeRequest sizeRequest = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);

            if (sizeRequest.Request.IsZero)
                return sizeRequest;

            switch (AspectEx)
            {
                case AspectExt.Uniform:
                      var innerAspectRatio = sizeRequest.Request.Width / sizeRequest.Request.Height;

                    if (double.IsInfinity(heightConstraint))
                    {
                        if (double.IsInfinity(widthConstraint))
                        {
                            // both destination constraints are infinity
                            // use the view's size request dimensions
                            widthConstraint = sizeRequest.Request.Width;
                            heightConstraint = sizeRequest.Request.Height;
                        }
                        else
                        {
                            // destination height constraint is infinity
                            heightConstraint = widthConstraint * sizeRequest.Request.Height / sizeRequest.Request.Width;
                        }
                    }
                    else if (double.IsInfinity(widthConstraint))
                    {
                        // destination width constraint is infity
                        widthConstraint = heightConstraint * sizeRequest.Request.Width / sizeRequest.Request.Height;
                    }
                    else
                    {
                        // both of the destination width and height constraints are non-infinity
                        var outerAspectRatio = widthConstraint / heightConstraint;

                        var resizeFactor = (innerAspectRatio >= outerAspectRatio) ?
                            (widthConstraint / sizeRequest.Request.Width) :
                            (heightConstraint / sizeRequest.Request.Height);

                        widthConstraint = sizeRequest.Request.Width * resizeFactor;
                        heightConstraint = sizeRequest.Request.Height * resizeFactor;
                    }
                    sizeRequest = new SizeRequest(new Size(widthConstraint, heightConstraint));
                    break;
                case AspectExt.None:
                    sizeRequest = new SizeRequest(new Size(sizeRequest.Request.Width / 2, sizeRequest.Request.Height / 2));
                    break;
            }

            return sizeRequest;
        }

        public enum AspectExt
        {
            /// <summary>Scale the image to fit the view. Some parts may be left empty (letter boxing).</summary>
            AspectFit,
            /// <summary>Scale the image to fill the view. Some parts may be clipped in order to fill the view.</summary>
            /// <remarks />
            AspectFill,
            /// <summary>Scale the image so it exactly fill the view. Scaling may not be uniform in X and Y.</summary>
            Fill,
            /// <summary>Scale the image to fill the view while it preserves its native aspect ratio.</summary>
            Uniform,
            /// <summary>The image preserves its original size.</summary>
            None
        }
    }
}