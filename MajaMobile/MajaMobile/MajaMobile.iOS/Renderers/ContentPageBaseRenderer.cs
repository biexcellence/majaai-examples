using MajaMobile.Pages;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPageBase), typeof(MajaMobile.iOS.Renderers.ContentPageBaseRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class ContentPageBaseRenderer : PageRenderer
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (ViewController?.NavigationController != null)
            {
                ViewController.NavigationController.InteractivePopGestureRecognizer.Enabled = true;
                try
                {
                    NavigationController.NavigationBar.BackItem.BackBarButtonItem = new UIBarButtonItem() { Title = "Zurück" };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot Set Back Button. Error: ", ex.Message);
                }
            }
        }
    }
}