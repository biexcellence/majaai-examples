using System;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(WebView), typeof(MajaMobile.iOS.Renderers.WebViewRendereriOS))]
namespace MajaMobile.iOS.Renderers
{
    class WebViewRendereriOS : WkWebViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                var webView = (WKWebView)NativeView;
                //webView.Opaque = false;
                //webView.BackgroundColor = UIKit.UIColor.Clear;
                //webView.ScrollView.BackgroundColor = UIKit.UIColor.Clear;
                webView.ScrollView.ScrollEnabled = false;
                webView.ScrollView.Bounces = false;
                webView.ScrollView.MaximumZoomScale = webView.ScrollView.MinimumZoomScale = 1;
                webView.NavigationDelegate = new NavDelegate(Webview_LoadFinished);
            }
        }

        private async void Webview_LoadFinished()
        {
            await System.Threading.Tasks.Task.Delay(100); // wait here till content is rendered
            var webView = (WKWebView)NativeView;
            Element.HeightRequest = webView.ScrollView.ContentSize.Height;
        }

        public class NavDelegate : WKNavigationDelegate
        {
            Action _loadFinished;

            public NavDelegate(Action loadFinished)
            {
                _loadFinished = loadFinished;
            }

            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                //base.DidFinishNavigation(webView, navigation);
                _loadFinished?.Invoke();
            }
        }
    }
}