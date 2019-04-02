using MajaMobile.Pages;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CancelBackContentPage), typeof(MajaMobile.iOS.Renderers.CancelBackPageRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class CancelBackPageRenderer : PageRenderer
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (ViewController?.NavigationController != null)
            {
                ViewController.NavigationController.InteractivePopGestureRecognizer.Enabled = false;
                try
                {
                    NavigationController.NavigationBar.BackItem.BackBarButtonItem = new UIBarButtonItem() { Title = "Abbrechen" };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot Set Back Button. Error: ", ex.Message);
                }
            }
        }
    }
}