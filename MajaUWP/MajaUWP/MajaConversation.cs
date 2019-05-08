using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace MajaUWP
{
    public class MajaConversation : PropertyChangedOnMainThread
    {
        public SessionHandler SessionHandler { get; }
        public ObservableCollection<ConversationMessage> Messages { get; } = new ObservableCollection<ConversationMessage>();
        public ObservableCollection<IPossibleUserReply> PossibleUserReplies { get; } = new ObservableCollection<IPossibleUserReply>();

        public event EventHandler<MajaQueryEventArgs> AnswersReceived;

        private MajaListeningStatus _majaStatus = MajaListeningStatus.Idle;
        public MajaListeningStatus MajaStatus
        {
            get => _majaStatus;
            set
            {
                _majaStatus = value;
                OnPropertyChanged();
                if (value == MajaListeningStatus.Thinking)
                    StartThinking();
                else
                    StopThinking();
            }
        }

        private UserConversationMessage _speechRecognitionMessage;

        private bool _dialogActive;
        public bool DialogActive
        {
            get => _dialogActive;
            set { _dialogActive = value; OnPropertyChanged(); }
        }

        public MajaConversation(SessionHandler sessionHandler)
        {
            SessionHandler = sessionHandler;
        }

        private MajaConversationMessage _thinkingMessage;
        private async void StartThinking()
        {
            if (_thinkingMessage == null)
            {
                var tcs = new TaskCompletionSource<bool>();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _thinkingMessage = new MajaConversationMessage(".");
                    Messages.Add(_thinkingMessage);
                    tcs.TrySetResult(true);
                });
                await tcs.Task;
                while (MajaStatus == MajaListeningStatus.Thinking && _thinkingMessage != null)
                {
                    var tmsg = _thinkingMessage;
                    if (tmsg != null)
                        tmsg.Text = tmsg.Text == "..." ? "." : tmsg.Text + ".";
                    await Task.Delay(350);
                }
            }
        }

        private async void StopThinking()
        {
            if (_thinkingMessage != null)
            {
                var tmsg = _thinkingMessage;
                if (tmsg != null)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Messages.Remove(_thinkingMessage));
                _thinkingMessage = null;
            }
        }

        public void StopListening()
        {
            _speechRecognitionMessage = null;
        }

        public async void SetUserText(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (_speechRecognitionMessage == null)
                    {
                        if (!DialogActive)
                        {
                            Messages.Clear();
                        }
                        _speechRecognitionMessage = new UserConversationMessage(text);
                        Messages.Add(_speechRecognitionMessage);
                    }
                    _speechRecognitionMessage.Text = text;
                }
            });
        }

        public async Task QueryMajaForAnswers(string value, string text = null, bool addMessage = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            _speechRecognitionMessage = null;
            if (MajaStatus == MajaListeningStatus.Thinking)
                return;
            if (!string.IsNullOrEmpty(value))
            {
                if (!DialogActive && addMessage)
                {
                    Messages.Clear();
                }
                PossibleUserReplies.Clear();
                if (addMessage)
                    Messages.Add(new UserConversationMessage(text ?? value));
                string speakingText = "";
                IList<IMajaQueryAnswer> answers = null;
                try
                {
                    MajaStatus = MajaListeningStatus.Thinking;
                    answers = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.QueryMajaForAnswers(value, Utils.MajaApiKey, Utils.MajaApiSecret, Utils.DefaultPackages, t), cancellationToken);
                    
                    if (answers == null || answers.Count == 0)
                    {
                        DialogActive = false;
                        speakingText = "Entschuldigung. Darauf habe ich keine Antwort.";
                        Messages.Add(new MajaConversationMessage(speakingText));
                    }
                    else
                    {
                        var completed = true;
                        speakingText = answers.First().Response;
                        foreach (var answer in answers)//TODO: only certain messages?
                        {
                            Messages.Add(new MajaConversationMessage(answer));
                            foreach (var possibleUserReply in answer.PossibleUserReplies)
                            {
                                if (string.Equals(possibleUserReply.ControlType, PossibleUserReplyControlType.Button, StringComparison.OrdinalIgnoreCase) || (!(string.IsNullOrEmpty(possibleUserReply.Value)) && !string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase)))
                                {
                                    PossibleUserReplies.Add(possibleUserReply);
                                }
                            }
                            completed = completed && answer.Completed;
                            //don't speak when answer is AudioFile or VideoFile
                            if (answer.ProposalType == MajaQueryAnswerProposalType.AudioFile || answer.ProposalType == MajaQueryAnswerProposalType.VideoFile)
                                speakingText = "";
                        }
                        DialogActive = !completed;
                    }
                }
                catch (OperationCanceledException)
                {
                    //DialogActive = false;
                    speakingText = "Die Anfrage wurde abgebrochen";
                    Messages.Add(new MajaConversationMessage(speakingText));
                }
                catch (Exception ex)
                {
                    DialogActive = false;
                    Messages.Add(new MajaConversationMessage(ex.Message));
                }
                finally
                {
                    MajaStatus = MajaListeningStatus.Idle;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        AnswersReceived?.Invoke(this, new MajaQueryEventArgs(answers, speakingText));
                    });
                }
            }

        }
    }

    public class ConversationMessage : PropertyChangedOnMainThread
    {
        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public string Image { get; protected set; }
        public MajaConversationSpeaker Speaker { get; }

        protected ConversationMessage(string text, MajaConversationSpeaker speaker)
        {
            Text = text;
            Speaker = speaker;
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
        public MajaConversationMessage(IMajaQueryAnswer queryAnswer) : base(queryAnswer.Response, MajaConversationSpeaker.Maja)
        {
            Text = queryAnswer.Response;
            Image = queryAnswer.Image;
        }
        public MajaConversationMessage(string text) : base(text, MajaConversationSpeaker.Maja)
        {

        }
    }

    public enum MajaConversationSpeaker
    {
        User,
        Maja
    }

    public enum MajaListeningStatus
    {
        Unknown,
        Idle,
        Thinking,
        Speaking,
        Listening,
    }

    public class MajaQueryEventArgs : EventArgs
    {
        public IList<IMajaQueryAnswer> Answers { get; }
        public string SpeakingText { get; }
        public MajaQueryEventArgs(IList<IMajaQueryAnswer> answers, string speakingText)
        {
            Answers = answers;
            SpeakingText = speakingText;
        }
    }
}