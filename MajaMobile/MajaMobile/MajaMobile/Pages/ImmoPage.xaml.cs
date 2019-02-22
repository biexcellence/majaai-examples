using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Messages;
using MajaMobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class ImmoPage : ContentPageBase
    {
        public ImmoPage(MajaConversationMessageImmo message)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ImmoPageViewModel(message);
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class ImmoPageViewModel : ViewModelBase
    {
        public ICommand ShowAllCommand { get; }
        public List<ImmoWrapper> Immos { get; } = new List<ImmoWrapper>();
        private string _url;

        public ImmoPageViewModel(MajaConversationMessageImmo message)
        {
            foreach (var obj in message.Immos)
            {
                Immos.Add(new ImmoWrapper(obj));
            }
            _url = message.MajaQueryAnswer.Url;
            ShowAllCommand = new Command(ShowAll);
        }

        public override void SendAppearing()
        {
            base.SendAppearing();
            MessagingCenter.Subscribe<ImmoWrapper>(this, ImmoWrapper.TappedMessage, ImmoTapped);
        }

        public override void SendDisappearing()
        {
            base.SendDisappearing();
            MessagingCenter.Unsubscribe<ImmoWrapper>(this, ImmoWrapper.TappedMessage);
        }

        private async void ShowAll()
        {
            if (!string.IsNullOrEmpty(_url))
            {
                try
                {
                    await Browser.OpenAsync(_url, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception) { }
            }
        }

        private async void ImmoTapped(ImmoWrapper wrapper)
        {
            try
            {
                await Browser.OpenAsync(wrapper.Immo.Href, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception) { }
        }

        public class ImmoWrapper
        {
            public const string TappedMessage = "OBJECT_TAPPED";

            public ICommand TappedCommand { get; }

            public RealEstateObject Immo { get; }

            public string PurchasePrice
            {
                get
                {
                    if (Immo.PurchasePrice == 0)
                        return "Preis auf Anfrage";
                    return string.Format("{0:N0} €", Immo.PurchasePrice);
                }
            }

            public ImmoWrapper(RealEstateObject obj)
            {
                Immo = obj;
                TappedCommand = new Command(() => MessagingCenter.Send(this, TappedMessage));
            }
        }
    }
}

namespace MajaMobile.Converters
{
    public class ImmoItemSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (double)value > 0 && int.TryParse(parameter?.ToString(), out int spacing))
            {
                return ((double)value / 2) - spacing;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && parameter is int)
            {
                return (int)value > (int)parameter;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}