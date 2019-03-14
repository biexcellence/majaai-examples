using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Models;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageFlightStatus : MajaConversationMessage
    {
        public FlightStatus FlightStatus { get; }

        public MajaConversationMessageFlightStatus(IMajaQueryAnswer queryAnswer, FlightStatus flightStatus) : base(queryAnswer)
        {
            FlightStatus = flightStatus;
        }
    }
}