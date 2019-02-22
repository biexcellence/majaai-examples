using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public abstract class ConversationMessage : INotifyPropertyChanged
    {
        public const string ConversationMessageTappedMessage = "MESSAGE_TAPPED";
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand TappedCommand { get; }
        public MajaConversationSpeaker Speaker { get; }
        public bool HasImage => !string.IsNullOrEmpty(Image);
        public virtual string Image { get; }

        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public ConversationMessage(string text, MajaConversationSpeaker speaker)
        {
            Text = text;
            Speaker = speaker;
            TappedCommand = new Command(MessageTapped);
        }

        protected virtual void MessageTapped()
        {

        }
    }

    public class UserConversationMessage : ConversationMessage
    {
        public UserConversationMessage(string text) : base(text, MajaConversationSpeaker.User)
        {

        }
    }

    public class MajaConversationMessage : ConversationMessage
    {
        public IMajaQueryAnswer MajaQueryAnswer { get; }
        public override string Image
        {
            get
            {
                if (MajaQueryAnswer != null && !string.IsNullOrEmpty(MajaQueryAnswer.Image))
                    return MajaQueryAnswer.Image;
                return null;
            }
        }

        public MajaConversationMessage(IMajaQueryAnswer queryAnswer) : base(queryAnswer.Response, MajaConversationSpeaker.Maja)
        {
            MajaQueryAnswer = queryAnswer;
        }

        /// <summary>
        /// For displaying exceptions
        /// </summary>
        /// <param name="text"></param>
        public MajaConversationMessage(string text) : base(text, MajaConversationSpeaker.Maja) { }

        public static MajaConversationMessage Factory(IMajaQueryAnswer queryAnswer)
        {
            switch (queryAnswer.ProposalType)
            {
                //case Message.MessageType.Image:
                //    return PhotoTemplate;
                case MajaQueryAnswerProposalType.Location:
                    return new MajaConversationMessageLocation(queryAnswer);
                case MajaQueryAnswerProposalType.VideoFile:
                    return new MajaConversationMessageVideo(queryAnswer);
                case MajaQueryAnswerProposalType.ImmoSuche:
                    return new MajaConversationMessageImmo(queryAnswer);
                case MajaQueryAnswerProposalType.AudioFile:
                    return new MajaConversationMessageAudio(queryAnswer);
                case MajaQueryAnswerProposalType.Simple when (string.Equals(queryAnswer.Action, MajaQueryAnswerAction.Weather, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(queryAnswer.Data):
                    return new MajaConversationMessageWeather(queryAnswer);
                case MajaQueryAnswerProposalType.Simple when (string.Equals(queryAnswer.Action, MajaQueryAnswerAction.News, StringComparison.OrdinalIgnoreCase)):
                    return new MajaConversationMessageNews(queryAnswer);
            }
            if (!string.IsNullOrEmpty(queryAnswer.Url))
            {
                return new MajaConversationMessageLink(queryAnswer);
            }
            return new MajaConversationMessage(queryAnswer);
        }

    }

    public enum MajaConversationSpeaker
    {
        User,
        Maja
    }
}