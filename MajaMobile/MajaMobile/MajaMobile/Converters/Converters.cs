using System;
using System.Collections;
using System.Globalization;
using Xamarin.Forms;

namespace MajaMobile.Converters
{
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            if (value is string s && string.IsNullOrWhiteSpace(s))
                return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ObjectFromListConverter : IValueConverter
    {
        /// <summary>
        /// get an object from an iList. parameter is index in iList
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list && parameter is string param && int.TryParse(param, out var index))
            {
                if (list.Count > index)
                {
                    return list[index];
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}