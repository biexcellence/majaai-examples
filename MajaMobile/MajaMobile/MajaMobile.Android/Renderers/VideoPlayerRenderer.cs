using Android.Content;
using Android.Widget;
using MajaMobile.Controls;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using RelativeLayoutDroid = Android.Widget.RelativeLayout;

[assembly: ExportRenderer(typeof(VideoPlayer), typeof(MajaMobile.Droid.Renderers.VideoPlayerRenderer))]
namespace MajaMobile.Droid.Renderers
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, RelativeLayoutDroid>
    {
        VideoView _videoView;
        MediaController _mediaController;    // Used to display transport controls

        public VideoPlayerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    // Save the VideoView for future reference
                    _videoView = new VideoView(Context);

                    // Put the VideoView in a RelativeLayout
                    RelativeLayoutDroid relativeLayout = new RelativeLayoutDroid(Context);
                    relativeLayout.AddView(_videoView);

                    // Center the VideoView in the RelativeLayout
                    RelativeLayoutDroid.LayoutParams layoutParams =
                        new RelativeLayoutDroid.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    layoutParams.AddRule(LayoutRules.CenterInParent);
                    _videoView.LayoutParameters = layoutParams;

                    // Handle a VideoView event
                    _videoView.Completion += OnVideoCompletion;

                    SetNativeControl(relativeLayout);
                }

                _mediaController = new MediaController(Context);
                _mediaController.SetMediaPlayer(_videoView);
                _videoView.SetMediaController(_mediaController);
                SetSource();
            }
        }

        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            if (Control != null && _videoView != null)
            {
                _videoView.Completion -= OnVideoCompletion;
            }
            base.Dispose(disposing);
        }


        private void OnVideoCompletion(object sender, EventArgs e)
        {
            if (!_disposed && _videoView != null)
            {
                _videoView.Start();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);
            if (args.PropertyName == VideoPlayer.SourceProperty.PropertyName)
            {
                SetSource();
            }
        }

        void SetSource()
        {
            string uri = Element.Source;

            if (!string.IsNullOrWhiteSpace(uri))
            {
                _videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                _videoView.Start();
            }
        }

    }
}