using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageVideo : MajaConversationMessage
    {
        public const string VideoTappedMessage = "VIDEO_TAPPED";
        public ICommand VideoTappedCommand { get; }

        public string VideoSource => MajaQueryAnswer.Action;

        public MajaConversationMessageVideo(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            VideoTappedCommand = new Command(() => MessagingCenter.Send(this, VideoTappedMessage));
        }
    }
}