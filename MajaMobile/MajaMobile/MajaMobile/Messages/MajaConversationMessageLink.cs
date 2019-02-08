using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageLink : MajaConversationMessage
    {
        public const string LinkTappedMessage = "LINK_TAPPED";
        public ICommand LinkTappedCommand { get; }
        public string Link => !string.IsNullOrEmpty(MajaQueryAnswer.UrlTitle) ? MajaQueryAnswer.UrlTitle : MajaQueryAnswer.Url;

        public MajaConversationMessageLink(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            LinkTappedCommand = new Command(() => MessagingCenter.Send(this, LinkTappedMessage));
        }
    }
}