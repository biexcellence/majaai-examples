using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Interfaces;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageHml : MajaConversationMessage
    {
        public string Html { get; }

        public double MessageWidth
        {
            get
            {
                return DependencyService.Get<IDeviceInfo>().ScreenWidth - 40;
            }
        }

        public MajaConversationMessageHml(IMajaQueryAnswer answer) : base(answer)
        {
            Html = "<!DOCTYPE html><html><head><style>@font-face { font-family: 'segoeui'; src: url('segoeui.ttf'); } html, body {margin: 0; padding: 0; background-color: #eeeeee; font-family: 'segoeui'; color: #706F6F;}</style></head><body>"
                 + answer.HtmlResponse
                 + "</body></html>";
        }
    }
}