using System.Collections.Generic;
using Xamarin.Forms;

namespace MajaMobile.Utilities
{
    public static class ColorScheme
    {

        public static readonly Color TextColor = Color.FromHex("706F6F"); //5d5d5d
        public static readonly Color NavigationBarColor = Color.FromHex("e4efef");
        public static readonly Color NavigationBarTextColor = Color.FromHex("1E8FCC"); //0066b3
        public static readonly Color EntryBackgroundColor = Color.White;
        public static readonly Color MajaMessageColor = Color.FromHex("eeeeee");
        public static readonly Color UserMessageColor = Color.FromHex("1E8FCC"); //419cc2
        public static readonly Color MajaMessageTextColor = Color.FromHex("706F6F");
        public static readonly Color UserMessageTextColor = Color.White;
        public static readonly Color SeparatorColor = Color.FromHex("cccccc");
        public static readonly Color SelectedItemColor = Color.FromHex("1E8FCC");
        public static readonly Color OverlayColor = Color.FromHex("aaffffff");

        public static readonly Color ChatButtonBackground = Color.FromHex("1E8FCC");
        public static readonly Color ChatButtonForeground = Color.White;

        public static Color ProgressBarTintColor = Color.FromHex("1E8FCC");
        public static Color ProgressBarBackgroundColor = Color.FromHex("aaffffff");

        public static Color Blue = Color.FromHex("72c9fb");
        public static Color DarkBlue = Color.FromHex("2893f6");
        public static Color LightBlue = Color.FromHex("def1fe");
        public static Color Green = Color.FromHex("a2c037");
        public static Color DarkGreen = Color.FromHex("33691e");
        public static Color LightGreen = Color.FromHex("c5e1a5");
        public static Color Gray = Color.FromHex("757575");
        public static Color DarkGray = Color.FromHex("3d3d3d");
        public static Color LightGray = Color.FromHex("e0e0e0");

        public static IList<Color> ChartColors = new List<Color>() { Green, DarkGreen, LightGreen, Blue, DarkBlue, LightBlue, Gray, DarkGray, LightGray };
    }
}