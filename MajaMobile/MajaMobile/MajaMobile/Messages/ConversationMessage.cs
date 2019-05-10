using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Interfaces;
using MajaMobile.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public abstract class ConversationMessage : INotifyPropertyChanged, IDisposable
    {
        public bool Disposed { get; private set; }

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

        public virtual double ImageWidth
        {
            get
            {
                return DependencyService.Get<IDeviceInfo>().ScreenWidth * 0.75;
            }
        }

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

        public virtual void Dispose()
        {
            Disposed = true;
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

        public static IEnumerable<MajaConversationMessage> Factory(IMajaQueryAnswer queryAnswer)
        {
            switch (queryAnswer.ProposalType)
            {
                case MajaQueryAnswerProposalType.Location:
                    return new[] { new MajaConversationMessageLocation(queryAnswer) };
                case MajaQueryAnswerProposalType.VideoFile:
                    return new[] { new MajaConversationMessageVideo(queryAnswer) };
                case MajaQueryAnswerProposalType.ImmoSuche:
                    return new[] { new MajaConversationMessageImmo(queryAnswer) };
                case MajaQueryAnswerProposalType.AudioFile:
                    return new[] { new MajaConversationMessageAudio(queryAnswer) };
                case MajaQueryAnswerProposalType.Simple when string.Equals(queryAnswer.Action, MajaQueryAnswerAction.Weather, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(queryAnswer.Data):
                    return new[] { new MajaConversationMessageWeather(queryAnswer) };
                case MajaQueryAnswerProposalType.Simple when string.Equals(queryAnswer.Action, MajaQueryAnswerAction.News, StringComparison.OrdinalIgnoreCase):
                    return new[] { new MajaConversationMessageNews(queryAnswer) };
                case MajaQueryAnswerProposalType.Simple when string.Equals(queryAnswer.Action, "flightstatus", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(queryAnswer.Data):
                    var list = new List<MajaConversationMessage>();
                    foreach (var flightStatus in FlightStatus.GetFlightStatusesFromJson(queryAnswer.Data))
                    {
                        list.Add(new MajaConversationMessageFlightStatus(queryAnswer, flightStatus));
                    }
                    if (list.Count > 0)
                        return list;
                    break;
                case MajaQueryAnswerProposalType.Simple when string.Equals(queryAnswer.Action, "poi", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(queryAnswer.Response) && queryAnswer.Entities.Any(e => string.Equals(e.EntityProvider, "poi", StringComparison.OrdinalIgnoreCase)):
                    return new[] { new MajaConversationMessagePoi(queryAnswer) };
                case MajaQueryAnswerProposalType.Link when !string.IsNullOrEmpty(queryAnswer.Url):
                    return new[] { new MajaConversationMessageLink(queryAnswer) };
            }
            if (!string.IsNullOrEmpty(queryAnswer.HtmlResponse))
                return new[] { new MajaConversationMessageHml(queryAnswer) };
            if (string.IsNullOrWhiteSpace(queryAnswer.Response))
                return new MajaConversationMessage[0];
            return new[] { new MajaConversationMessage(queryAnswer) };
        }

    }

    public enum MajaConversationSpeaker
    {
        User,
        Maja
    }
}