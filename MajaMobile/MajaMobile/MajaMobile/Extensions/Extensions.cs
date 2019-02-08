using Plugin.Share.Abstractions;
using System;
using Xamarin.Forms;

namespace MajaMobile.Extensions
{
    internal static class Extensions
    {
        public static ShareColor ToShareColor(this Color color)
        {
            return new ShareColor(Convert.ToInt32(255 * color.R), Convert.ToInt32(255 * color.G), Convert.ToInt32(255 * color.B), Convert.ToInt32(255 * color.A));
        }
    }
}