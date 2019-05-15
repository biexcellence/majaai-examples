using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class CustomWebView : WebView
    {

        public static readonly BindableProperty MajaCommandProperty = BindableProperty.Create(nameof(MajaCommand), typeof(ICommand), typeof(CustomWebView));
        public ICommand MajaCommand
        {
            get { return (ICommand)GetValue(MajaCommandProperty); }
            set { SetValue(MajaCommandProperty, value); }
        }

        public event EventHandler<WebNavigatingEventArgs> NavigatingFixed;

        public void SendNavigatingEvent(WebNavigatingEventArgs args)
        {
            NavigatingFixed?.Invoke(this, args);
        }

        public CustomWebView()
        {
            //https://github.com/xamarin/Xamarin.Forms/pull/5458
            if (Device.RuntimePlatform == Device.Android)
                NavigatingFixed += CustomWebView_Navigating;
            else
                Navigating += CustomWebView_Navigating;
        }

        private void CustomWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            try
            {
                var uri = new Uri(e.Url);
                if (uri.Host == "localhost")
                {
                    e.Cancel = true;
                    var value = Uri.UnescapeDataString(uri.Query.Substring("?value=".Length));
                    MajaCommand?.Execute(value);
                }
            }
            catch { }
        }
    }
}
