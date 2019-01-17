using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.ViewModels;
using MajaUWP.WeatherControls;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class WeatherPage : Page
    {
        public WeatherPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is WeatherForecast forecast)
            {
                DataContext = new WeatherViewModel(forecast);
            }
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class WeatherViewModel : ViewModelBase
    {
        public WeatherForecast Forecast { get; }
        public WeatherDetails TodayWeather => Forecast?.FirstOrDefault();
        public string Time { get; }
        public string City { get; }
        public WeatherViewModel(WeatherForecast forecast)
        {
            Forecast = forecast;
            City = forecast.City;
            Time = DateTime.Now.ToString("HH:00");
        }
    }
}
namespace MajaUWP.Converters
{
    public class WeatherControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is WeatherDetails weather)
            {
                if (weather.SymbolNumber >= 200 && weather.SymbolNumber < 300)
                {
                    return new ThunderstormControl();
                }
                else if (weather.SymbolNumber >= 300 && weather.SymbolNumber < 400)
                {
                    if (weather.IsNightTime)
                        return new NightRainShowersControl();
                    return new RainShowersControl();
                }
                else if (weather.SymbolNumber >= 400 && weather.SymbolNumber < 500)
                {
                    return "Unknown weather";
                }
                else if (weather.SymbolNumber >= 500 && weather.SymbolNumber < 600)
                {
                    return new RainyControl();
                }
                else if (weather.SymbolNumber >= 600 && weather.SymbolNumber < 700)
                {
                    return new SnowyControl();
                }
                else if (weather.SymbolNumber >= 700 && weather.SymbolNumber < 800)
                {
                    return new FoggyControl();
                }
                else if (weather.SymbolNumber >= 800 && weather.SymbolNumber < 900)
                {
                    switch (weather.SymbolNumber)
                    {
                        case 800 when weather.IsNightTime:
                            return new MoonControl();
                        case 800:
                            return new SunControl();
                        case 801 when weather.IsNightTime:
                            return new NightPartlyCloudyControl();
                        case 801:
                            return new PartlyCloudyControl();
                        case 802 when weather.IsNightTime:
                        case 803 when weather.IsNightTime:
                            return new NightPartlyCloudyExtendedControl();
                        case 802:
                        case 803:
                            return new PartlyCloudyExtendedControl();
                        default:
                            if (weather.IsNightTime)
                                return new CloudyGrayControl();
                            return new CloudyWhiteControl();
                    }
                }
            }
            return "Unknown weather";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateToDayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
                if (parameter == null)
                    return DateTimeFormatInfo.CurrentInfo.GetDayName(date.DayOfWeek);
                else
                    return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(date.DayOfWeek);
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class WindAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int dir)
                return (double)Math.Abs(dir - 180);
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}