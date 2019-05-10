using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Android.Webkit;
using static Android.Views.View;
using Android.Views;
using Android.Content;

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
                var client = new DynamicHeightWebViewClient(Element);
                Control.SetOnTouchListener(client);
                Control.SetWebViewClient(client);
            }
        }
    }
    class DynamicHeightWebViewClient : WebViewClient, IValueCallback, IOnTouchListener
    {
        private readonly Xamarin.Forms.WebView _webView;

        public DynamicHeightWebViewClient(Xamarin.Forms.WebView webView)
            : base()
        {
            _webView = webView;
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