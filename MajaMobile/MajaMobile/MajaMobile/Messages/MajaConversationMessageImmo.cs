using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageImmo : MajaConversationMessage
    {
        public  ICommand ImmosTappedCommand { get; }

        private IList<RealEstateObject> _immos;
        public IList<RealEstateObject> Immos
        {
            get
            {
                if (_immos == null && !string.IsNullOrEmpty(MajaQueryAnswer.Data))
                    _immos = RealEstateObject.GetListFromData(MajaQueryAnswer.Data);
                return _immos;
            }
        }

        public string ImmoLinkText => $"{Immos.Count}{(Immos.Count>=10 ? "+" : "")} Immobilien gefunden";

        public MajaConversationMessageImmo(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            ImmosTappedCommand = new Command(() => MessagingCenter.Send(this, ConversationMessageTappedMessage));
        }
    }
}