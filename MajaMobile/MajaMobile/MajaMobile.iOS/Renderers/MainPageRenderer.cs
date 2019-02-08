using Foundation;
using MajaMobile;
using SoftInput.iOS.Render;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MainPage), typeof(KeyboardRender))]
namespace SoftInput.iOS.Render
{

    [Preserve(AllMembers = true)]
    public class KeyboardRender : PageRenderer
    {
        NSObject _keyboardShowObserver;
        NSObject _keyboardHideObserver;
        private bool _isKeyboardShown;

        public KeyboardRender() { }

        public static new void Init()
        {

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var page = Element as ContentPage;

            if (page != null)
            {
                var contentScrollView = page.Content as ScrollView;

                if (contentScrollView != null)
                    return;

                RegisterForKeyboardNotifications();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UnregisterForKeyboardNotifications();
        }

        void RegisterForKeyboardNotifications()
        {
            if (_keyboardShowObserver == null)
                _keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardShow);
            if (_keyboardHideObserver == null)
                _keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardHide);
        }

        void UnregisterForKeyboardNotifications()
        {
            _isKeyboardShown = false;
            if (_keyboardShowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardShowObserver);
                _keyboardShowObserver.Dispose();
                _keyboardShowObserver = null;
            }

            if (_keyboardHideObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardHideObserver);
                _keyboardHideObserver.Dispose();
                _keyboardHideObserver = null;
            }
        }

        protected virtual void OnKeyboardShow(NSNotification notification)
        {
            if (!IsViewLoaded || _isKeyboardShown)
                return;

            _isKeyboardShown = true;
            var activeView = View.FindFirstResponder();

            if (activeView == null)
                return;

            var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);

            ((MainPage)Element).ShiftEntryUp(keyboardFrame.Height);
        }

        private void OnKeyboardHide(NSNotification notification)
        {
            if (!IsViewLoaded)
                return;

            _isKeyboardShown = false;

            ((MainPage)Element).ShiftEntryDown();
        }
    }

    public static class ViewExtensions
    {

        public static UIView FindFirstResponder(this UIView view)
        {
            if (view.IsFirstResponder)
            {
                return view;
            }
            foreach (UIView subView in view.Subviews)
            {
                var firstResponder = subView.FindFirstResponder();
                if (firstResponder != null)
                    return firstResponder;
            }
            return null;
        }
    }
}