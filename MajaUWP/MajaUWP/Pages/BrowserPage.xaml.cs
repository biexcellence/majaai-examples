using System;
using Windows.UI.Xaml.Navigation;

namespace MajaUWP.Pages
{
    public sealed partial class BrowserPage : MajaPage
    {
        public BrowserPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Uri uri)
            {
                try
                {
                    Browser.Navigate(uri);
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message);
                }
            }
            else if (e.Parameter is string s)
            {
                try
                {
                    Browser.NavigateToString(s);
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message);
                }
            }
        }
    }
}