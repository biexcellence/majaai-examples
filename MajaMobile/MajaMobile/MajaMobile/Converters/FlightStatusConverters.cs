using MajaMobile.Models;
using MajaMobile.Utilities;
using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace MajaMobile.Converters
{
    public class FlightDelayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlightStatus status)
            {
                if (status.Status == FlightStatusCodes.Landed && targetType == typeof(string))
                    return "Gelandet";
                if (status.ArrivalGateDelayMinutes > 15)
                {
                    if (targetType == typeof(Color))
                        return ColorScheme.FlightColorRed;
                    return string.Format("{0} Min. Verspätung", status.ArrivalGateDelayMinutes);
                }
                switch (status.Status)
                {
                    case FlightStatusCodes.Canceled:
                        if (targetType == typeof(Color))
                        {
                            if (parameter != null)
                                return ColorScheme.MajaMessageTextColor;
                            return ColorScheme.FlightColorRed;
                        }
                        return "Gestrichen";
                    case FlightStatusCodes.Scheduled:
                        //TODO: what if not on time and which fields to check
                        if (status.DepartureGateDelayMinutes > 15)
                        {
                            if (targetType == typeof(Color))
                                return ColorScheme.FlightColorRed;
                            return string.Format("{0} Min. Verspätung", status.DepartureGateDelayMinutes);
                        }
                        if (targetType == typeof(Color))
                            return ColorScheme.MajaMessageTextColor;
                        return "Planmäßig";
                }
                if (targetType == typeof(Color))
                    return ColorScheme.FlightColorGreen;
                return "Pünktlich";
            }
            if (targetType == typeof(Color))
                return ColorScheme.MajaMessageTextColor;
            return "Keine Informationen";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// first parameter: 'arr': arrivalDateLabel, 'dep': departureDateLabel
    /// </summary>
    public class FlightStatusTimeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlightStatus status)
            {
                if ((string)parameter == "dep")
                {
                    if (status.ActualGateDeparture.HasValue && status.ScheduledGateDeparture.HasValue && status.ActualGateDeparture.Value < status.ScheduledGateDeparture.Value)
                        return true;
                    return status.DepartureGateDelayMinutes > 0;
                }
                if (status.Status == FlightStatusCodes.Active && status.EstimatedGateArrival.HasValue && status.ScheduledGateArrival.HasValue && status.EstimatedGateArrival.Value < status.ScheduledGateArrival.Value)
                    return true;
                if (status.Status == FlightStatusCodes.Landed && status.ActualGateArrival.HasValue && status.ScheduledGateArrival.HasValue && status.ActualGateArrival.Value < status.ScheduledGateArrival.Value)
                    return true;
                return status.ArrivalGateDelayMinutes > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// parameter: 'arr': arrivalDateLabel, 'dep': departureDateLabel
    /// </summary>
    public class FlightStatusActualDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlightStatus status)
            {
                switch ((string)parameter)
                {
                    case "dep" when status.ActualGateDeparture.HasValue:
                        return status.ActualGateDeparture.Value.ToString("HH:mm");
                    case "dep" when status.EstimatedGateDeparture.HasValue:
                        return status.EstimatedGateDeparture.Value.ToString("HH:mm");
                    case "dep" when status.ScheduledGateDeparture.HasValue:
                        return status.ScheduledGateDeparture.Value.ToString("HH:mm");
                    case "dep" when status.DepartureDate.HasValue:
                        return status.DepartureDate.Value.ToString("HH:mm");
                    case "arr" when status.ActualGateArrival.HasValue:
                        return status.ActualGateArrival.Value.ToString("HH:mm");
                    case "arr" when status.EstimatedGateArrival.HasValue:
                        return status.EstimatedGateArrival.Value.ToString("HH:mm");
                    case "arr" when status.ScheduledGateArrival.HasValue:
                        return status.ScheduledGateArrival.Value.ToString("HH:mm");
                    case "arr" when status.ArrivalDate.HasValue:
                        return status.ArrivalDate.Value.ToString("HH:mm");
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FirstWordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
                return s.Split(' ').FirstOrDefault();
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}