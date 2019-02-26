using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Controls;
using MajaMobile.Interfaces;
using MajaMobile.Messages;
using MajaMobile.Pages;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class MainPage : ContentPageBase
    {
        public bool IsIdle => ViewModel.IsIdle;

        public MainPage()
        {
            InitializeComponent();
            var viewmodel = new MainPageViewModel();
            viewmodel.SendingText += Viewmodel_SendingText;
            BindingContext = ViewModel = viewmodel;
        }

        private void Viewmodel_SendingText(object sender, EventArgs e)
        {
            ChatControl.UnfocusCurrentControl();
        }

        public void ShiftEntryUp(double keyboardHeight)
        {
            MainGrid.TranslationY = keyboardHeight * -1;
        }

        public void ShiftEntryDown()
        {
            MainGrid.TranslationY = 0;
        }
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

namespace MajaMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand LongPressedCommand { get; }
        public ICommand ReleasedCommand { get; }
        public ICommand SendTextCommand { get; }
        public ICommand PossibleUserReplyCommand { get; }

        public event EventHandler SendingText;

        IAudioService _audioService;
        IDeviceInfo _deviceInfo;

        private MajaConversationMessageThinking _thinkingMessage;
        private UserConversationMessage _speechRecognitionMessage;
        private bool _dialogActive;
        public IPossibleUserReply CurrentUserInput
        {
            get => GetField<IPossibleUserReply>();
            private set { SetField(value); }
        }
        public ObservableCollection<ConversationMessage> Messages { get; } = new ObservableCollection<ConversationMessage>();
        public ObservableCollection<IPossibleUserReply> PossibleUserReplies { get; } = new ObservableCollection<IPossibleUserReply>();

        public string Text
        {
            get => GetField<string>();
            set
            {
                SetField(value);
                UpdateChatButton();
            }
        }

        public ChatButtonDisplayMode ChatButtonMode
        {
            get => GetField<ChatButtonDisplayMode>();
            set { SetField(value); }
        }

        public MajaListeningStatus CurrentMajaState
        {
            get => GetField<MajaListeningStatus>();
            set
            {
                SetField(value);
                MajaStateChanged(value);
            }
        }

        public MainPageViewModel()
        {
            _deviceInfo = DependencyService.Get<IDeviceInfo>();
            _audioService = DependencyService.Get<IAudioService>();

            CurrentMajaState = MajaListeningStatus.Idle;
            ChatButtonMode = ChatButtonDisplayMode.Microphone;
            LongPressedCommand = new Command(LongPressed);
            ReleasedCommand = new Command(Released);
            SendTextCommand = new Command(SendTextCommandExecuted);
            PossibleUserReplyCommand = new Command(PossibleUserReplyTapped);

            _audioService.CompletedAudio += _audioService_CompletedAudio;
            _audioService.StartedAudio += _audioService_StartedAudio;
            _audioService.SpeechRecognitionPartialResult += _audioService_SpeechRecognitionPartialResult;
            _audioService.SpeechRecognitionResult += _audioService_SpeechRecognitionResult;
        }

        private void SendTextCommandExecuted(object parameter)
        {
            if (parameter is DateTime date)
            {
                var value = date.ToString(BiExcellence.OpenBi.Api.Internal.Utils.DateTimeOffsetToString(new DateTimeOffset(date)));
                var text = date.ToString("D");
                SendText(value, text);
            }
            else if (parameter is IMajaEntity entity)
            {
                var dict = new Dictionary<string, string>();
                dict["ENTITY_ID"] = entity.Id;
                SendText(entity.Name, parameters: dict);
            }
            else
            {
                SendText();
            }
        }

        public override void SendDisappearing()
        {
            base.SendDisappearing();
            foreach (MajaConversationMessageAudio audioMessage in Messages.Where(m => m is MajaConversationMessageAudio))
            {
                if (audioMessage.IsPlaying)
                {
                    audioMessage.StopAudio();
                }
            }
            StopAudioService();
        }

        private void StopAudioService()
        {
            CurrentMajaState = MajaListeningStatus.Idle;
            _speechRecognitionMessage = null;
            _audioService.StopService();
        }

        private void _audioService_SpeechRecognitionResult(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Results == null || _speechRecognitionMessage == null)
            {
                CurrentMajaState = MajaListeningStatus.Idle;
                return;
            }
            string text = "", value = "";
            if (CurrentUserInput != null)
            {
                foreach (var result in e.Results)
                {
                    if (string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Integer, StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(result, out int i))
                        {
                            value = i.ToString();
                        }
                    }
                    else if (string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Double, StringComparison.OrdinalIgnoreCase))
                    {
                        if (double.TryParse(result, out double d))
                        {
                            text = d.ToString(CultureInfo.CurrentUICulture);
                            value = d.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    else if (string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Date, StringComparison.OrdinalIgnoreCase))
                    {
                        if (DateTime.TryParse(result, out DateTime date))
                        {
                            value = date.ToString(BiExcellence.OpenBi.Api.Internal.Utils.DateTimeOffsetToString(new DateTimeOffset(date)));
                            text = date.ToString("D");
                        }
                    }
                    else
                    {
                        value = result;
                    }
                    if (!string.IsNullOrEmpty(value))
                        break;
                }
            }
            if (string.IsNullOrEmpty(value))
                value = e.Results?.FirstOrDefault();
            if (string.IsNullOrEmpty(text))
                text = value;
            if (_speechRecognitionMessage != null)
                _speechRecognitionMessage.Text = text;
            if (!string.IsNullOrEmpty(_speechRecognitionMessage?.Text))
                SendText(value, text, addMessage: false);
        }

        private void _audioService_SpeechRecognitionPartialResult(object sender, SpeechRecognitionEventArgs e)
        {
            var text = e.Results?.FirstOrDefault();
            if (!string.IsNullOrEmpty(text))
            {
                if (_speechRecognitionMessage == null)
                {
                    if (!_dialogActive)
                    {
                        Messages.Clear();
                    }
                    _speechRecognitionMessage = new UserConversationMessage(text);
                    Messages.Add(_speechRecognitionMessage);
                }
                _speechRecognitionMessage.Text = text;
            }
        }

        private void _audioService_StartedAudio(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("_audioService_StartedAudio");
        }

        private void _audioService_CompletedAudio(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("_audioService_StartedAudio");
        }

        private void MajaStateChanged(MajaListeningStatus state)
        {
            IsIdle = state == MajaListeningStatus.Idle;
            if (state == MajaListeningStatus.Thinking)
            {
                _thinkingMessage = new MajaConversationMessageThinking();
                Messages.Add(_thinkingMessage);
            }
            else
            {
                var message = _thinkingMessage;
                _thinkingMessage = null;
                if (message != null)
                {
                    message.Dispose();
                    Messages.Remove(message);
                }
            }
            UpdateChatButton();
        }

        private void PossibleUserReplyTapped(object obj)
        {
            if (IsBusy)
                return;
            if (CurrentMajaState == MajaListeningStatus.Listening)
            {
                var speechMessage = _speechRecognitionMessage;
                StopAudioService();
                if (speechMessage != null)
                    Messages.Remove(speechMessage);
            }
            if (obj is IPossibleUserReply possibleUserReply)
                SendText(possibleUserReply.Value, possibleUserReply.Text);
        }

        private void UpdateChatButton()
        {
            ChatButtonDisplayMode buttonMode = ChatButtonDisplayMode.Undefined;
            if (CurrentMajaState != MajaListeningStatus.Listening)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    buttonMode = ChatButtonDisplayMode.Microphone;
                }
                else
                {
                    buttonMode = ChatButtonDisplayMode.Send;
                }
            }
            else
            {
                buttonMode = ChatButtonDisplayMode.Listening;
            }
            if (buttonMode != ChatButtonMode)
                ChatButtonMode = buttonMode;
        }

        private async void SendText(string value = null, string text = null, IDictionary<string, string> parameters = null, bool addMessage = true)
        {
            _speechRecognitionMessage = null;
            if (IsBusy)
                return;
            if (string.IsNullOrEmpty(value))
                value = Text;
            if (!string.IsNullOrEmpty(value))
            {
                CurrentUserInput = null;
                if (!_dialogActive && addMessage)
                {
                    Messages.Clear();
                }
                PossibleUserReplies.Clear();
                if (addMessage)
                    Messages.Add(new UserConversationMessage(text ?? value));
                CurrentMajaState = MajaListeningStatus.Thinking;
                CancellationTokenSource tokenSource = _thinkingMessage?.CancellationTokenSource;
                Text = "";
                SendingText?.Invoke(this, EventArgs.Empty);
                try
                {
                    IList<IMajaQueryAnswer> answers = null;
                    var cancellationToken = tokenSource != null ? tokenSource.Token : default(CancellationToken);
                    answers = await SessionHandler.Instance.ExecuteOpenbiCommand((s, t) => s.QueryMajaForAnswers(value, Utils.MajaApiKey, Utils.MajaApiSecret, SessionHandler.Packages, parameters, t), cancellationToken);
                    if (tokenSource == null || !tokenSource.IsCancellationRequested)
                    {
                        if (answers == null || answers.Count == 0)
                        {
                            _dialogActive = false;
                            Messages.Add(new MajaConversationMessage("Entschuldigung. Darauf habe ich keine Antwort."));
                        }
                        else
                        {
                            var completed = true;
                            foreach (var answer in answers)//TODO: only certain messages?
                            {
                                Messages.Add(MajaConversationMessage.Factory((answer)));
                                foreach (var possibleUserReply in answer.PossibleUserReplies)
                                {
                                    if (string.Equals(possibleUserReply.ControlType, PossibleUserReplyControlType.Button, StringComparison.OrdinalIgnoreCase) || (!(string.IsNullOrEmpty(possibleUserReply.Value)) && !string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        PossibleUserReplies.Add(possibleUserReply);
                                    }
                                    else
                                    {
                                        CurrentUserInput = possibleUserReply;
                                    }
                                }
                                completed = completed && answer.Completed;
                            }
                            _dialogActive = !completed;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _dialogActive = false;
                    Messages.Add(new MajaConversationMessage("Die Anfrage wurde abgebrochen"));
                }
                catch (Exception ex)
                {
                    _dialogActive = false;
                    Messages.Add(new MajaConversationMessage(ex.Message));
                }
                finally
                {
                    CurrentMajaState = MajaListeningStatus.Idle;
                }
            }
        }

        private void Released(object obj)
        {
            if (IsIdle && ChatButtonMode == ChatButtonDisplayMode.Listening)
            {
                _audioService.StopService();
                ChatButtonMode = ChatButtonDisplayMode.Microphone;
            }
            else if (ChatButtonMode == ChatButtonDisplayMode.Send)
                SendText();
        }

        private void LongPressed(object obj)
        {
            if (IsIdle && ChatButtonMode == ChatButtonDisplayMode.Microphone)
            {
                _audioService.StartSpeechRecognition();
                ChatButtonMode = ChatButtonDisplayMode.Listening;
                //try
                //{
                //    Vibration.Vibrate();
                //}
                //catch { }
            }
        }
    }
}