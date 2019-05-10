using System;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(WebView), typeof(MajaMobile.iOS.Renderers.WebViewRendereriOS))]
namespace MajaMobile.iOS.Renderers
{
    class WebViewRendereriOS : WebViewRenderer
    {
        class WebViewDelegate : UIWebViewDelegate
        {
            private readonly IUIWebViewDelegate _base;
            private readonly WebViewRenderer _renderer;

            public WebViewDelegate(IUIWebViewDelegate @base, WebViewRenderer renderer)
            {
                _base = @base;
                _renderer = renderer;
            }

            public override void LoadFailed(UIWebView webView, NSError error)
            {
                _base.LoadFailed(webView, error);
            }

            public override void LoadingFinished(UIWebView webView)
            {
                _base.LoadingFinished(webView);

                var frame = webView.Frame;
                frame.Height = 1;
                webView.Frame = frame;

                var scrollHeight = webView.EvaluateJavascript("document.body.offsetHeight");
                if (int.TryParse(scrollHeight, out var height))
                {
                    var element = _renderer.Element;
                    if (element != null && element.HeightRequest != height)
                    {
                        frame = webView.Frame;
                        frame.Height = height;
                        webView.Frame = frame;
                        element.HeightRequest = height;
                    }
                }
            }

            public override void LoadStarted(UIWebView webView)
            {
                _base.LoadStarted(webView);
            }

            public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
            {
                return _base.ShouldStartLoad(webView, request, navigationType);
            }
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                var webView = (UIWebView)NativeView;
                webView.ScrollView.ScrollEnabled = false;
                webView.ScrollView.Bounces = false;

                webView.Delegate = new WebViewDelegate(Delegate, this);
            }
        }
    }
}