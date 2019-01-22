using BiExcellence.OpenBi.Api;
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
        private const string MajaApiKey = "";
        private const string MajaApiSecret = "";
        private IOpenBiSession _openbiSession;
        private SpeechRecognitionService _speechRecognitionService;
        private static IOpenBiConfiguration _openBiConfiguration = new OpenBiConfiguration(Protocol.HTTPS, "maja.ai", 443, "Maja UWP");

        public ObservableCollection<MajaConversationMessage> Messages { get; } = new ObservableCollection<MajaConversationMessage>();

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

        //private object _lock = new object();
        private MajaConversationMessage _userMessage;

        private IMajaQueryAnswer _majaQueryAnswer;
        public IMajaQueryAnswer MajaQueryAnswer
        {
            get => _majaQueryAnswer;
            set { _majaQueryAnswer = value; OnPropertyChanged(); }
        }

        public MajaConversation(SpeechRecognitionService speechRecognitionService)
        {
            _openbiSession = new OpenBiSession(_openBiConfiguration);
            _speechRecognitionService = speechRecognitionService;
        }


        private MajaConversationMessage _thinkingMessage;

        private async void StartThinking()
        {
            if (_thinkingMessage == null)
            {
                var tcs = new TaskCompletionSource<bool>();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _thinkingMessage = new MajaConversationMessage(".", MajaConversationSpeaker.Maja);
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


        public async void SetUserText(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_clearMessages || (MajaQueryAnswer != null && MajaQueryAnswer.Completed))
                {
                    Messages.Clear();
                    _clearMessages = false;
                }
                MajaQueryAnswer = null;
                MajaConversationMessage userMessage = _userMessage;
                if (userMessage == null)
                {
                    //lock (_lock)
                    //if (_userMessage == null)
                    //{
                    userMessage = _userMessage = new MajaConversationMessage(text, MajaConversationSpeaker.User);
                    Messages.Add(userMessage);
                    //}
                }
                _userMessage.Text = text;
            });
        }

        public async Task<MajaListeningStatus> SendMajaQuery(string value, string text = null, bool speak = true)
        {
            try
            {
                await _speechRecognitionService.StopMaja();
            }
            catch { }
            MajaListeningStatus status = MajaListeningStatus.Unknown;
            try
            {
                status = await _speechRecognitionService.SendMajaQuery(value, text = null, speak);
                if (status == MajaListeningStatus.Listening)
                {
                    if (speak)
                        _speechRecognitionService.StartSpeechRecognition();
                    else
                        MajaStatus = MajaListeningStatus.Idle;
                }
                else
                {
                    MajaStatus = status;
                }
            }
            catch (Exception)
            {
                MajaStatus = MajaListeningStatus.Idle;
            }
            return status;
        }

        private bool _clearMessages;

        public async Task<IList<IMajaQueryAnswer>> QueryMajaForAnswers(string value, string text = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_clearMessages || (MajaQueryAnswer != null && MajaQueryAnswer.Completed))
            {
                Messages.Clear();
                _clearMessages = false;
            }
            MajaConversationMessage userMessage = _userMessage;
            if (userMessage != null)
                userMessage.Text = text ?? value;
            else if (Messages.Count == 0 || Messages.Last().Speaker == MajaConversationSpeaker.Maja)
                Messages.Add(new MajaConversationMessage(value, MajaConversationSpeaker.User));
            _userMessage = null;
            MajaQueryAnswer = null;

            try
            {
                MajaStatus = MajaListeningStatus.Thinking;
                IList<IMajaQueryAnswer> answers = null;
                try
                {
                    answers = await _openbiSession.QueryMajaForAnswers(value, MajaApiKey, MajaApiSecret, cancellationToken);
                }
                catch (OpenBiServerErrorException requestEx) when (requestEx.Response.Code == -97)
                {
                    _openbiSession = new OpenBiSession(_openBiConfiguration);
                    answers = await _openbiSession.QueryMajaForAnswers(value, MajaApiKey, MajaApiSecret, cancellationToken);
                }
                MajaQueryAnswer = answers.FirstOrDefault();
                if (answers.Count == 0)
                {
                    Messages.Add(new MajaConversationMessage("Entschuldigung. Darauf habe ich keine Antwort.", MajaConversationSpeaker.Maja));
                    _clearMessages = true;
                }
                else
                {
                    foreach (var answer in answers)//TODO: only certain messages?
                        Messages.Add(new MajaConversationMessage(answer));
                }
                return answers;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Messages.Add(new MajaConversationMessage(ex.Message, MajaConversationSpeaker.Maja));
                throw new MajaServerException();
            }
            finally
            {
                MajaStatus = MajaListeningStatus.Processing;
            }
        }
    }

    public class MajaConversationMessage : PropertyChangedOnMainThread
    {
        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public string Image { get; }
        public MajaConversationSpeaker Speaker { get; }

        public MajaConversationMessage(string text, MajaConversationSpeaker speaker)
        {
            Text = text;
            Speaker = speaker;
        }

        public MajaConversationMessage(IMajaQueryAnswer queryAnswer)
        {
            Text = queryAnswer.Response;
            Speaker = MajaConversationSpeaker.Maja;
            Image = queryAnswer.Image;
        }
    }

    public class MajaServerException : Exception
    {

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
        Processing,
    }
}