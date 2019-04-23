using AVFoundation;
using AVKit;
using Foundation;
using MajaMobile.Controls;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(VideoPlayer), typeof(MajaMobile.iOS.Renderers.VideoPlayerRenderer))]

namespace MajaMobile.iOS.Renderers
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, UIView>
    {
        AVPlayer _player;
        AVPlayerItem _playerItem;
        AVPlayerViewController _playerViewController;       // solely for ViewController property

        public override UIViewController ViewController => _playerViewController;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    // Create AVPlayerViewController
                    _playerViewController = new AVPlayerViewController();

                    // Set Player property to AVPlayer
                    _player = new AVPlayer();
                    _playerViewController.Player = _player;

                    // Use the View from the controller as the native control
                    SetNativeControl(_playerViewController.View);
                }

                ((AVPlayerViewController)ViewController).ShowsPlaybackControls = true;
                SetSource();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_player != null)
            {
                _player.ReplaceCurrentItemWithPlayerItem(null);
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
                var asset = AVAsset.FromUrl(new NSUrl(uri));
                _playerItem = new AVPlayerItem(asset);
            }
            else
            {
                _playerItem = null;
            }

            _player.ReplaceCurrentItemWithPlayerItem(_playerItem);

            if (_playerItem != null)
            {
                _player.Play();
            }
        }
    }
}