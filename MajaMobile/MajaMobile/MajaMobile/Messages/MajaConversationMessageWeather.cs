using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageWeather : MajaConversationMessage
    {
        public const string WeatherTappedMessage = "WEATHER_TAPPED";
        public ICommand WeatherTappedCommand { get; }

        private WeatherForecast _forecast;
        public WeatherForecast Weather
        {
            get
            {
                if (_forecast == null && !string.IsNullOrEmpty(MajaQueryAnswer.Data))
                    _forecast = (WeatherForecast)MajaQueryAnswer.DeserializeData();
                return _forecast;
            }
        }

        public MajaConversationMessageWeather(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            WeatherTappedCommand = new Command(() => MessagingCenter.Send(this, WeatherTappedMessage));
        }
    }
}