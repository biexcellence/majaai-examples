using BiExcellence.OpenBi.Api.Commands.MajaAi;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageImage : MajaConversationMessage
    {
        public string Image => MajaQueryAnswer.Image;
        public MajaConversationMessageImage(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
        }
    }
}