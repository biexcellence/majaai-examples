using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.ViewModels;
using System;
using System.Linq;

namespace MajaMobile.Pages
{
    public partial class WeatherPage : ContentPageBase
    {
        public WeatherPage(WeatherForecast forecast)
        {
            InitializeComponent();
            BindingContext = ViewModel = new WeatherPageViewModel(forecast);
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class WeatherPageViewModel : ViewModelBase
    {
        public WeatherForecast Forecast { get; }

        private WeatherDetails _currentWeather;
        public WeatherDetails CurrentWeather
        {
            get => _currentWeather;
            set
            {
                _currentWeather = value;
                OnPropertyChanged();
            }
        }
        public string Time => DateTime.Now.ToString("HH:00");
        public WeatherPageViewModel(WeatherForecast forecast)
        {
            Forecast = forecast;
            CurrentWeather = forecast.FirstOrDefault();
        }
    }
}