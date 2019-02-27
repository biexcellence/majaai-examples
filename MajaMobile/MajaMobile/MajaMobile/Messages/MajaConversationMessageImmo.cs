using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageImmo : MajaConversationMessage
    {
        public ICommand ImmosTappedCommand { get; }

        public List<ImmoObject> Immos { get; } = new List<ImmoObject>();

        public string ImmoLinkText => $"{Immos.Count} Immobilien gefunden";

        public MajaConversationMessageImmo(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            ImmosTappedCommand = new Command(() => MessagingCenter.Send(this, ConversationMessageTappedMessage));
            foreach (var entity in queryAnswer.Entities.Where(e => string.Equals(e.EntityProvider, "immobilienProvider", System.StringComparison.OrdinalIgnoreCase)))
            {
                Immos.Add(new ImmoObject(entity));
            }
        }
    }
}