using Foundation;
using MajaMobile.Controls;
using MajaMobile.GestureRecognizers;
using SkiaSharp.Views.Forms;
using System.Linq;
using UIKit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(ChatButton), typeof(MajaMobile.iOS.Renderers.ChatButtonRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class ChatButtonRenderer : SKCanvasViewRenderer
    {

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            foreach (var recognizer in Element.GestureRecognizers.Where(g => g is PressedGestureRecognizer))
            {
                var pressedRecognizer = (PressedGestureRecognizer)recognizer;
                pressedRecognizer.Command?.Execute(pressedRecognizer.CommandParameter);
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            foreach (var recognizer in Element.GestureRecognizers.Where(g => g is ReleasedGestureRecognizer))
            {
                var releasedRecognizer = (ReleasedGestureRecognizer)recognizer;
                releasedRecognizer.Command?.Execute(releasedRecognizer.CommandParameter);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            foreach (var recognizer in Element.GestureRecognizers.Where(g => g is ReleasedGestureRecognizer))
            {
                var releasedRecognizer = (ReleasedGestureRecognizer)recognizer;
                releasedRecognizer.Command?.Execute(releasedRecognizer.CommandParameter);
            }
        }
    }
}