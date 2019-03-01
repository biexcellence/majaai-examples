using Android.Content;
using MajaMobile.GestureRecognizers;
using SkiaSharp.Views.Forms;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MajaMobile.Controls.ChatButton), typeof(MajaMobile.Droid.Renderers.ChatButtonRenderer))]
namespace MajaMobile.Droid.Renderers
{
    public class ChatButtonRenderer : SKCanvasViewRenderer
    {
        public ChatButtonRenderer(Context context) : base(context)
        {
        }


        protected override void OnElementChanged(ElementChangedEventArgs<SKCanvasView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null && e.NewElement.GestureRecognizers.Count > 0)
            {
                if (e.NewElement.GestureRecognizers.Any(g => g is PressedGestureRecognizer || g is ReleasedGestureRecognizer))
                {
                    Control.LongClick += Control_LongClick;
                    Control.Touch += Control_Touch;
                }
            }
        }

        private void Control_LongClick(object sender, LongClickEventArgs e)
        {
            System.Console.WriteLine("Control_LongClick");
            foreach (var recognizer in Element.GestureRecognizers.Where(g => g is PressedGestureRecognizer))
            {
                var pressedRecognizer = (PressedGestureRecognizer)recognizer;
                pressedRecognizer.Command?.Execute(pressedRecognizer.CommandParameter);
            }
        }

        private void Control_Touch(object sender, TouchEventArgs e)
        {
            if (e.Event.Action == Android.Views.MotionEventActions.ButtonRelease)
            {
                System.Console.WriteLine("ButtonRelease");
            }
            else if (e.Event.Action == Android.Views.MotionEventActions.ButtonPress)
            {
                System.Console.WriteLine("ButtonPress");
            }
            else if (e.Event.Action == Android.Views.MotionEventActions.Down)
            {
                foreach (var recognizer in Element.GestureRecognizers.Where(g => g is PressedGestureRecognizer))
                {
                    var pressedRecognizer = (PressedGestureRecognizer)recognizer;
                    pressedRecognizer.Command?.Execute(pressedRecognizer.CommandParameter);
                }
            }
            else if (e.Event.Action == Android.Views.MotionEventActions.Up)
            {
                foreach (var recognizer in Element.GestureRecognizers.Where(g => g is ReleasedGestureRecognizer))
                {
                    var releasedRecognizer = (ReleasedGestureRecognizer)recognizer;
                    releasedRecognizer.Command?.Execute(releasedRecognizer.CommandParameter);
                }
            }
        }
    }
}