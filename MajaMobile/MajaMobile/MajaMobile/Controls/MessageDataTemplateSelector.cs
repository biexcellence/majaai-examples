using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Messages;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class MessageDataTemplateSelector : DataTemplateSelector
    {

        public DataTemplate TextTemplate { get; set; }
        public DataTemplate UrlTemplate { get; set; }
        public DataTemplate RealEstateTemplate { get; set; }
        public DataTemplate NewsTemplate { get; set; }
        public DataTemplate WeatherTemplate { get; set; }
        public DataTemplate LocationTemplate { get; set; }
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate ThinkingTemplate { get; set; }
        public DataTemplate FlightStatusTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var message = item as ConversationMessage;
            if (message is MajaConversationMessageAudio)
                return AudioTemplate;
            if (message is MajaConversationMessageImmo)
                return RealEstateTemplate;
            if (message is MajaConversationMessageLink)
                return UrlTemplate;
            if (message is MajaConversationMessageLocation)
                return LocationTemplate;
            if (message is MajaConversationMessageVideo)
                return VideoTemplate;
            if (message is MajaConversationMessageNews)
                return NewsTemplate;
            if (message is MajaConversationMessageWeather)
                return WeatherTemplate;
            if (message is MajaConversationMessageThinking)
                return ThinkingTemplate;
            if (message is MajaConversationMessageFlightStatus flightStatus && flightStatus.FlightStatus != null)
                return FlightStatusTemplate;
            return TextTemplate;
        }
    }

    public class PossibleUserReplyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is IPossibleUserReply reply && reply.ControlOptions.TryGetValue("IMAGE", out var img) && !string.IsNullOrEmpty((string)img))
                return ImageTemplate;
            return TextTemplate;
        }
    }
}