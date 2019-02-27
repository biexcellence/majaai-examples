using MajaMobile.Messages;
using MajaMobile.ViewModels;
using System;
using System.Globalization;
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
        public MajaConversationMessageImmo Message { get; }

        public ImmoPageViewModel(MajaConversationMessageImmo message)
        {
            Message = message;
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