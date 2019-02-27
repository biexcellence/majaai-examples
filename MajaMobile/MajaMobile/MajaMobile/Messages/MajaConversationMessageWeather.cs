using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;
using System.Linq;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageWeather : MajaConversationMessage
    {
        public WeatherForecast Forecast { get; }

        private WeatherDetails _currentWeather;
        public WeatherDetails CurrentWeather
        {
            get => _currentWeather;
            set { _currentWeather = value; OnPropertyChanged(); }
        }
        public string Time => DateTime.Now.ToString("HH:00");

        public MajaConversationMessageWeather(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            Forecast = new WeatherForecast(queryAnswer.Data);
            CurrentWeather = Forecast.FirstOrDefault();
        }
    }
}