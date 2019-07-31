using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MajaUWP.Converters
{
    public class UserRepliesButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UserReply reply && reply.ControlType == PossibleUserReplyControlType.Button)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class UserRepliesDateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UserReply repyl && repyl.ControlType == PossibleUserReplyControlType.Text && repyl.Type == "DATE")
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class UserRepliesFileVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value is UserReply repyl && repyl.ControlType == PossibleUserReplyControlType.Text && repyl.Type == "FILE")
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    


}
