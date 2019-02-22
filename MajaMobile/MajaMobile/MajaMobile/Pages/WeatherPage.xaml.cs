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

        public WeatherDetails CurrentWeather
        {
            get => GetField<WeatherDetails>();
            set { SetField(value); }
        }
        public string Time => DateTime.Now.ToString("HH:00");
        public WeatherPageViewModel(WeatherForecast forecast)
        {
            Forecast = forecast;
            CurrentWeather = forecast.FirstOrDefault();
        }
    }
}