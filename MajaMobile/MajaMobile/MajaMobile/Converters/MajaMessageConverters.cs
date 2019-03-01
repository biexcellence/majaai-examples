using MajaMobile.Messages;
using MajaMobile.Utilities;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace MajaMobile.Converters
{
    public class SpeakerToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MajaConversationSpeaker speaker && speaker == MajaConversationSpeaker.User)
                return LayoutOptions.EndAndExpand;
            return LayoutOptions.StartAndExpand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SpeakerToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MajaConversationSpeaker speaker && speaker == MajaConversationSpeaker.User)
                return new Thickness(40, 10, 10, 10);
            return new Thickness(10, 10, 40, 10);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SpeakerToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MajaConversationSpeaker speaker && speaker == MajaConversationSpeaker.User)
                return ColorScheme.UserMessageColor;
            return ColorScheme.MajaMessageColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SpeakerToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MajaConversationSpeaker speaker && speaker == MajaConversationSpeaker.User)
                return ColorScheme.UserMessageTextColor;
            return ColorScheme.MajaMessageTextColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageToImageRowSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return 5.0;
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class DateToDayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                if (parameter == null)
                    return DateTimeFormatInfo.CurrentInfo.GetDayName(date.DayOfWeek);
                else
                    return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(date.DayOfWeek) + ".";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}