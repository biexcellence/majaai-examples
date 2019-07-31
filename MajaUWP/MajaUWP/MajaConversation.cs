using BiExcellence.OpenBi.Api.Commands.MajaAi;
using GalaSoft.MvvmLight.Messaging;
using MajaUWP.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;

namespace MajaUWP
{
    public class MajaConversation : PropertyChangedOnMainThread
    {
        public SessionHandler SessionHandler { get; }
        public ObservableCollection<ConversationMessage> Messages { get; } = new ObservableCollection<ConversationMessage>();
        public ObservableCollection<UserReply> PossibleUserReplies { get; } = new ObservableCollection<UserReply>();

        public SpeechRecognitionService SpeechRecognitionService { get; }

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

                if (value == MajaListeningStatus.Listening)
                {
                    Messenger.Default.Send<string>("playStartDing");
                }
            }
        }

        private UserConversationMessage _speechRecognitionMessage;
        public CancellationTokenSource cts = new CancellationTokenSource();

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
            cts.Dispose();
            cts = new CancellationTokenSource();
            cancellationToken = cts.Token;

            _speechRecognitionMessage = null;
            if (MajaStatus == MajaListeningStatus.Thinking)
                return;
            if (value.EndsWith("."))
            { value = value.Remove(value.Length - 1); }
            if (value.EndsWith("?"))
            { value = value.Remove(value.Length - 1); }

            string speakingText = "";
            IList<IMajaQueryAnswer> answers = null;
            try
            {
                
               

                if (!string.IsNullOrEmpty(value))
                {
                    if (!DialogActive && addMessage)
                    {
                        Messages.Clear();
                    }
                    PossibleUserReplies.Clear();
                    if (addMessage)
                        Messages.Add(new UserConversationMessage(text ?? value));



                    MajaStatus = MajaListeningStatus.Thinking;
                    Dictionary<string, string> emptyToken = new Dictionary<string, string>() { { "token", "" } };

                    answers = await SessionHandler.ExecuteOpenbiCommand((s, t) =>  s.QueryMajaForAnswers(value, Utils.MajaApiKey, Utils.MajaApiSecret, Utils.MajaPackages, emptyToken, t),cancellationToken);

                    

                    //Add microsoftToken if requested
                    if (answers.Any(m => m.Data == "returnToken") && Utils.microSoftToken != null && Utils.microSoftToken != "")
                    {
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters.Add("token", Utils.microSoftToken);
                        answers = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.QueryMajaForAnswers(value, Utils.MajaApiKey, Utils.MajaApiSecret, Utils.MajaPackages, parameters, t), cancellationToken);
                    }

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
                            //possible User replies sorting
                            foreach (var possibleUserReply in answer.PossibleUserReplies)
                            {
                                //convert Entities to Buttons
                                if (string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity))
                                {
                                    var entities = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetMajaEntities(possibleUserReply.ControlOptions["ENTITY_ID"].ToString(), possibleUserReply.ControlOptions["ENTITY_FILTER"].ToString(), "", Utils.MajaApiKey, Utils.MajaApiSecret, Utils.MajaPackages, null, t));
                                    if (entities.Count < 10)
                                    {
                                        foreach (var entity in entities)
                                        {
                                            PossibleUserReplies.Add(new UserReply(entity.DisplayAttributes["NAME"].ToString(), "STRING", entity.DisplayAttributes["Name"].ToString(), "BUTTON", new Dictionary<string, object>()));
                                        }
                                    }
                                }
                                if (possibleUserReply.Type == "STRING")
                                {
                                    PossibleUserReplies.Add(new UserReply(possibleUserReply.Text, "STRING", possibleUserReply.Value, "BUTTON", new Dictionary<string, object>()));
                                }
                                //Categories
                                if (possibleUserReply.Type == "TEXT" && possibleUserReply.Type == "TEXT")
                                {
                                    PossibleUserReplies.Add(new UserReply(possibleUserReply.Text, "TEXT", possibleUserReply.Value, "BUTTON", new Dictionary<string, object>()));
                                }
                                //Phrases
                                if (possibleUserReply.Type == "PHRASE")
                                {
                                    PossibleUserReplies.Add(new UserReply(possibleUserReply.Value, "STRING", possibleUserReply.Value, "BUTTON", new Dictionary<string, object>()));
                                }
                                else
                                {
                                    PossibleUserReplies.Add(new UserReply(possibleUserReply.Text, possibleUserReply.Type, possibleUserReply.Value, possibleUserReply.ControlType, possibleUserReply.ControlOptions));
                                }

                                //No filtering all are added

                                //if (string.Equals(possibleUserReply.ControlType, PossibleUserReplyControlType.Button, StringComparison.OrdinalIgnoreCase) || (!(string.IsNullOrEmpty(possibleUserReply.Value)) && !string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase)))
                                //{
                                //    PossibleUserReplies.Add(new UserReply(possibleUserReply.Text, possibleUserReply.Type, possibleUserReply.Value, possibleUserReply.ControlType,possibleUserReply.ControlOptions));
                                //}
                                //if (string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity))
                                //{
                                //    var entities = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetMajaEntities(possibleUserReply.ControlOptions["ENTITY_ID"].ToString(), possibleUserReply.ControlOptions["ENTITY_FILTER"].ToString(), "", Utils.MajaApiKey, Utils.MajaApiSecret, Utils.DefaultPackages,null,t));
                                //    if (entities.Count < 10)
                                //    {
                                //        foreach (var entity in entities)
                                //        {
                                //            PossibleUserReplies.Add(new UserReply(entity.DisplayAttributes["NAME"].ToString(), "STRING", entity.DisplayAttributes["Name"].ToString(), "BUTTON", new Dictionary<string, object>()));
                                //        }
                                //    }
                                //}
                                //if (possibleUserReply.ControlType == PossibleUserReplyControlType.Text)
                                //{
                                //    if (possibleUserReply.Type == "DATE")
                                //    {
                                //        PossibleUserReplies.Add(new UserReply(possibleUserReply.Text, possibleUserReply.Type, possibleUserReply.Value, possibleUserReply.ControlType, possibleUserReply.ControlOptions));
                                //    }


                                //}
                            }
                            completed = completed && answer.Completed;
                            //don't speak when answer is AudioFile or VideoFile
                            if (answer.ProposalType == MajaQueryAnswerProposalType.AudioFile || answer.ProposalType == MajaQueryAnswerProposalType.VideoFile)
                                speakingText = "";
                        }
                        DialogActive = !completed;
                    }
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


       
       


    

    public class ConversationMessage : PropertyChangedOnMainThread
    {
        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public string Data { get; protected set; }
        public string Html { get; protected set; }
        public string Image { get; protected set; }
        public MajaConversationSpeaker Speaker { get; }

        public ConversationMessage(string text, MajaConversationSpeaker speaker)
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
            if (string.IsNullOrEmpty(queryAnswer.HtmlResponse))
            {
                Text = queryAnswer.Response;
            }
            else
            {
                Text = "";
                Html = queryAnswer.HtmlResponse;
            }
            Image = queryAnswer.Image;
            Data = queryAnswer.Data;
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

    public class UserReply {
        public string Text { get; protected set; }
        public string Type { get; protected set; }
        public string Value { get; protected set; }
        public string ControlType { get; protected set; }
        public IDictionary<string, object> ControlOptions { get; protected set; }
        public UserReply(string _text, string _type, string _value, string _ctrlType, IDictionary<string,object> _controlOptions)
        {
            Text = _text;
            Type = _type;
            Value = _value;
            ControlType = _ctrlType;
            ControlOptions = _controlOptions;
        }

    }
}