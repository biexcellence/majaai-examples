using MajaMobile.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(IOSEntry), typeof(MajaMobile.iOS.Renderers.IOSEntryRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class IOSEntryRenderer : EntryRenderer
    {
        private bool _handlersSet;
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (!_handlersSet)
            {
                _handlersSet = true;
                Element.PropertyChanged += Element_PropertyChanged;
            }
        }

        private void Element_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Xamarin.Forms.InputView.KeyboardProperty.PropertyName)
            {
                var keyboardtype = Control.KeyboardType;
                if (Element.Keyboard == Keyboard.Numeric)
                {
                    Control.KeyboardType = UIKit.UIKeyboardType.NumberPad;
                }
                else
                {
                    Control.KeyboardType = UIKit.UIKeyboardType.Default;
                }
            }
        }
    }
}