using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace MajaMobile.Converters
{
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = false;
            if (value == null)
                result = false;
            else if (value is string s && string.IsNullOrWhiteSpace(s))
                result = false;
            else
                result = true;
            return parameter == null ? result : !result;
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

    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is byte[] bytes)
            {
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}