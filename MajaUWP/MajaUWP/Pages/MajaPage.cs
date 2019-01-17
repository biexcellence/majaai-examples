using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace MajaUWP.Pages
{
    public class MajaPage : Page
    {

        protected async void ShowMessage(string message)
        {
            if (Dispatcher.HasThreadAccess)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                await messageDialog.ShowAsync();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                    await messageDialog.ShowAsync();
                });
            }
        }
    }
}