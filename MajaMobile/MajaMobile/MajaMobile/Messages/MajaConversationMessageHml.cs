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
            Html = "<!DOCTYPE html><html><head><style>@font-face { font-family: 'segoeui'; src: url('segoeui.ttf'); } html, body {margin: 0; padding: 0; background-color: #eeeeee; font-family: 'segoeui'; color: #706F6F;} .majaai-reply { /*ios hack...*/ cursor: pointer; }</style></head><body>"
                 + answer.HtmlResponse
                 + @"<script>document.body.addEventListener(""click"", function(e) { if (e.target.classList.contains(""majaai-reply"")) { var value = e.target.dataset[""value""] || e.target.value || e.target.textContent; location.href = ""http://localhost/clicked?value="" + encodeURIComponent(value); }  }, false);</script>"
                 + "</body></html>";
        }
    }
}