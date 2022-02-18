using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class CustomFlexLayout : FlexLayout
    {

        //protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        //{
        //    if (Device.RuntimePlatform == Device.Android && !double.IsInfinity(heightConstraint))
        //    {
        //        foreach (var c in Children)
        //        {
        //            var m = c.Measure(0, 0, MeasureFlags.IncludeMargins);
        //            heightConstraint = m.Request.Height * 4;
        //        }
        //    }
        //    return base.OnMeasure(widthConstraint, heightConstraint);
        //}

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    if (height > 0)
        //        height = height / 2;
        //    base.OnSizeAllocated(width, height);
        //}

    }
}