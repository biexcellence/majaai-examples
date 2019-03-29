using MajaMobile.Pages;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(UserProfilePage), typeof(MajaMobile.iOS.Renderers.UserProfilePageRenderer))]
namespace MajaMobile.iOS.Renderers
{
    public class UserProfilePageRenderer : PageRenderer
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