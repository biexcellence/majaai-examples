using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class UserConversationMessageMultipleChoice : UserConversationMessage
    {
        public const string MultipleChoiceTappedMessage = "MULTIPLE_CHOICE_TAPPED";

        public ObservableCollection<IPossibleUserReply> PossibleReplies { get; } = new ObservableCollection<IPossibleUserReply>();
        public ICommand ReplyTappedCommand { get; }

        public UserConversationMessageMultipleChoice(string text, IList<IPossibleUserReply> possibleUserReplies) : base(text)
        {
            foreach (var reply in possibleUserReplies)
            {
                PossibleReplies.Add(reply);
            }
            ReplyTappedCommand = new Command(ReplyTapped);
        }

        private void ReplyTapped(object obj)
        {
            MessagingCenter.Send(this, MultipleChoiceTappedMessage, (IPossibleUserReply)obj);
        }
    }
}