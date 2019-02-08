using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public class NavigationPageBase : NavigationPage
    {

        public NavigationPageBase(Page root) : base(root)
        {
            var style = (Style)Application.Current.Resources["ContentPageStyle"];
            Style = style;
            BarBackgroundColor = Utilities.ColorScheme.NavigationBarColor;
            BarTextColor = Utilities.ColorScheme.NavigationBarTextColor;
        }

    }
}
