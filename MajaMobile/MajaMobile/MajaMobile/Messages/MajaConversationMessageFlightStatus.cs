using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Interfaces;
using MajaMobile.Models;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageFlightStatus : MajaConversationMessage
    {
        public FlightStatus FlightStatus { get; }

        public double MessageWidth
        {
            get
            {
                return DependencyService.Get<IDeviceInfo>().ScreenWidth - 40;
            }
        }

        public override double ImageWidth => MessageWidth;

        public MajaConversationMessageFlightStatus(IMajaQueryAnswer queryAnswer, FlightStatus flightStatus) : base(queryAnswer)
        {
            FlightStatus = flightStatus;
        }
    }
}