using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Android.Webkit;
using static Android.Views.View;
using Android.Views;
using Android.Content;
using Android.Graphics;
using MajaMobile.Controls;

[assembly: ExportRenderer(typeof(Xamarin.Forms.WebView), typeof(MajaMobile.Droid.Renderers.WebViewRendererDroid))]
namespace MajaMobile.Droid.Renderers
{
    class WebViewRendererDroid:WebViewRenderer
    {
        public WebViewRendererDroid(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                var client = new DynamicHeightWebViewClient(this);
                Control.SetOnTouchListener(client);
                Control.SetWebViewClient(client);
            }
        }
    }
    class DynamicHeightWebViewClient : FormsWebViewClient, IValueCallback, IOnTouchListener
    {
        private readonly WebViewRenderer _renderer;
        private readonly Xamarin.Forms.WebView _webView;

        public DynamicHeightWebViewClient(WebViewRenderer renderer)
            : base(renderer)
        {
            _webView = renderer.Element;
            _renderer = renderer;
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            if (_renderer.Element is CustomWebView customWebView)
            {
                var args = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url);

                customWebView.SendNavigatingEvent(args);

                if (args.Cancel)
                {
                    view.StopLoading();
                    return;
                }
            }

            base.OnPageStarted(view, url, favicon);
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            base.OnPageFinished(view, url);
            view.EvaluateJavascript("document.body.scrollHeight", this);
        }

        public void OnReceiveValue(Java.Lang.Object value)
        {
            if (int.TryParse(value.ToString(), out var height))
            {
                if (_webView.HeightRequest != height)
                {
                    _webView.HeightRequest = height;
                }
            }
        }

        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            return e.Action == MotionEventActions.Move;
        }
    }
}