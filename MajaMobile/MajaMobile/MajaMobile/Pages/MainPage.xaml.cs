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

        public MainPage(SessionHandler sessionHandler)
        {
            InitializeComponent();
            var viewmodel = new MainPageViewModel(sessionHandler);
            BindingContext = ViewModel = viewmodel;
        }

        public void ShiftEntryUp(double keyboardHeight)
        {
            ChatButton.TranslationY = ChatControl.TranslationY = MultipleChoiceControl.TranslationY = CancelLabel.TranslationY = keyboardHeight * -1;
        }

        public void ShiftEntryDown()
        {
            ChatButton.TranslationY = ChatControl.TranslationY = MultipleChoiceControl.TranslationY = CancelLabel.TranslationY = 0;
        }

        #region AutoComplete
        private string _lastSearch = "";
        private CancellationTokenSource _previousCts;
        private async void UpdateEntityInfo(string text)
        {
            text = text.Trim();
            var possibleUserReply = ((MainPageViewModel)ViewModel).CurrentUserInput;
            if (possibleUserReply == null || !string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase) || !possibleUserReply.ControlOptions.TryGetValue("ENTITY_ID", out var entityId))
                return;
            if (text.Length < 3)
            {
                CancelRunningTask();
                ChatControl.EntitySearchResults.Clear();
                _lastSearch = "";
                return;
            }
            if (!string.Equals(_lastSearch, text, StringComparison.OrdinalIgnoreCase))
            {
                _lastSearch = text;
                try
                {
                    CancelRunningTask();
                    var cts = _previousCts = new CancellationTokenSource();

                    string filter = "";
                    if (possibleUserReply.ControlOptions.TryGetValue("ENTITY_FILTER", out var entityFilter))
                    {
                        filter = (string)entityFilter;
                    }
                    var entities = await ViewModel.SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetMajaEntities((string)entityId, filter, text, Utils.MajaApiKey, Utils.MajaApiSecret, ViewModel.SessionHandler.Packages, null, t), cts.Token);
                    if (!cts.IsCancellationRequested)
                    {
                        _previousCts = null;
                        ChatControl.EntitySearchResults.Clear();
                        foreach (var entity in entities?.Take(10).OrderBy(e => e.Name))
                        {
                            ChatControl.EntitySearchResults.Add(entity);
                        }
                    }
                }
                catch (Exception e) { }
            }
        }

        private void CancelRunningTask()
        {
            try
            {
                var previousCts = _previousCts;
                _previousCts = null;
                if (previousCts != null)
                {
                    try
                    {
                        previousCts.Cancel();
                    }
                    catch { }
                }
            }
            catch (Exception) { }
        }

        private void ChatControl_AutoCompleteValueChanged(object sender, Syncfusion.SfAutoComplete.XForms.ValueChangedEventArgs e)
        {
            UpdateEntityInfo(e.Value);
        }

        private void ChatControl_AutoCompleteselectionChanged(object sender, Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs e)
        {
            CancelRunningTask();
        }
        #endregion
    }

    public enum MajaListeningStatus
    {
        Unknown,
        Idle,
        Thinking,
        Listening,
    }
}

namespace MajaMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand SpeechRecognitionCommand { get; }
        public ICommand SendTextCommand { get; }
        public ICommand PossibleUserReplyCommand { get; }
        public ICommand MajaSpeakingEnabledCommand { get; }
        public ICommand CancelDialogCommand { get; }

        IAudioService _audioService;
        IDeviceInfo _deviceInfo;

        private MajaConversationMessageThinking _thinkingMessage;
        private UserConversationMessage _speechRecognitionMessage;
        public bool DialogActive
        {
            get => GetField<bool>();
            private set { SetField(value); OnPropertyChanged(); }
        }
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

        public bool MajaSpeakingEnabled
        {
            get => GetField<bool>();
            set { SetField(value); }
        }

        public MainPageViewModel(SessionHandler sessionHandler) : base(sessionHandler)
        {
            _deviceInfo = DependencyService.Get<IDeviceInfo>();
            _audioService = DependencyService.Get<IAudioService>();

            CurrentMajaState = MajaListeningStatus.Idle;
            ChatButtonMode = ChatButtonDisplayMode.Microphone;
            SpeechRecognitionCommand = new Command(SpeechRecognition);
            SendTextCommand = new Command(SendTextCommandExecuted);
            PossibleUserReplyCommand = new Command(PossibleUserReplyTapped);
            MajaSpeakingEnabledCommand = new Command(SwitchMajaSpeakingEnabled);
            CancelDialogCommand = new Command(CancelDialog);

            MajaSpeakingEnabled = true;
        }

        public override void SendAppearing()
        {
            base.SendAppearing();
            _audioService.SpeechRecognitionPartialResult += _audioService_SpeechRecognitionPartialResult;
            _audioService.SpeechRecognitionResult += _audioService_SpeechRecognitionResult;
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
            _audioService.SpeechRecognitionPartialResult -= _audioService_SpeechRecognitionPartialResult;
            _audioService.SpeechRecognitionResult -= _audioService_SpeechRecognitionResult;
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
                    if (!DialogActive)
                    {
                        Messages.Clear();
                    }
                    _speechRecognitionMessage = new UserConversationMessage(text);
                    Messages.Add(_speechRecognitionMessage);
                }
                _speechRecognitionMessage.Text = text;
            }
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
            if (CurrentMajaState == MajaListeningStatus.Listening)
            {
                ChatButtonMode = ChatButtonDisplayMode.Listening;
            }
            else
            {
                if (string.IsNullOrEmpty(Text))
                {
                    ChatButtonMode = ChatButtonDisplayMode.Microphone;
                }
                else
                {
                    ChatButtonMode = ChatButtonDisplayMode.Send;
                }
            }
        }

        private async void SendText(string value = null, string text = null, IDictionary<string, string> parameters = null, bool addMessage = true)
        {
            _speechRecognitionMessage = null;
            if (IsBusy && CurrentMajaState != MajaListeningStatus.Listening && !_cancellingDialog)
                return;
            if (string.IsNullOrEmpty(value))
                value = Text;
            if (!string.IsNullOrEmpty(value))
            {
                CurrentUserInput = null;
                if (!DialogActive && addMessage)
                {
                    Messages.Clear();
                }
                PossibleUserReplies.Clear();
                if (addMessage)
                    Messages.Add(new UserConversationMessage(text ?? value));
                CurrentMajaState = MajaListeningStatus.Thinking;
                CancellationTokenSource tokenSource = _thinkingMessage?.CancellationTokenSource;
                Text = "";
                _audioService.StopAudio();
                string speakingText = "";
                try
                {
                    IList<IMajaQueryAnswer> answers = null;
                    var cancellationToken = tokenSource != null ? tokenSource.Token : default(CancellationToken);
                    answers = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.QueryMajaForAnswers(value, Utils.MajaApiKey, Utils.MajaApiSecret, SessionHandler.Packages, parameters, t), cancellationToken);
                    if (tokenSource == null || !tokenSource.IsCancellationRequested)
                    {
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
                                foreach (var message in MajaConversationMessage.Factory((answer)))
                                {
                                    Messages.Add(message);
                                }
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
                                //don't speak when answer is AudioFile
                                if (answer.ProposalType == MajaQueryAnswerProposalType.AudioFile)
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
                    CurrentMajaState = MajaListeningStatus.Idle;
                    if (MajaSpeakingEnabled && !string.IsNullOrEmpty(speakingText))
                    {
                        _audioService.PlayAudio(speakingText);
                    }
                    _cancellingDialog = false;
                }
                TextSent();
            }
        }

        protected virtual void TextSent() { }

        private bool _cancellingDialog;
        private void CancelDialog()
        {
            if (_cancellingDialog)
                return;
            _cancellingDialog = true;
            var tokenSource = _thinkingMessage?.CancellationTokenSource;
            if (tokenSource != null)
            {
                try
                {
                    tokenSource.Cancel();
                }
                catch { }
            }
            SendText("Abbrechen", "Dialog abbrechen");
        }

        private void SwitchMajaSpeakingEnabled()
        {
            MajaSpeakingEnabled = !MajaSpeakingEnabled;
            if (!MajaSpeakingEnabled)
            {
                _audioService.StopAudio();
            }
        }

        private void SpeechRecognition()
        {
            switch (ChatButtonMode)
            {
                case ChatButtonDisplayMode.Send:
                    SendText();
                    break;
                case ChatButtonDisplayMode.Listening:
                    StopAudioService();
                    break;
                case ChatButtonDisplayMode.Microphone:
                    if (IsIdle)
                    {
                        _audioService.StartSpeechRecognition();
                        CurrentMajaState = MajaListeningStatus.Listening;
                    }
                    break;
            }
        }
    }
}

namespace MajaMobile.Converters
{
    public class PossibleUserReplyImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IPossibleUserReply reply && reply.ControlOptions.TryGetValue("IMAGE", out var img))
                return (string)img;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}