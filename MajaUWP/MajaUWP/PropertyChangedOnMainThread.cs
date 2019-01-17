using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

namespace MajaUWP
{
    public class PropertyChangedOnMainThread : INotifyPropertyChanged
    {
        protected CoreDispatcher Dispatcher => Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
        public event PropertyChangedEventHandler PropertyChanged;
        protected async void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (Dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
            }
        }

    }
}
