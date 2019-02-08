using BiExcellence.OpenBi.Api;
using BiExcellence.OpenBi.Api.Commands.Entities;
using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Controls;
using MajaMobile.Extensions;
using MajaMobile.Interfaces;
using MajaMobile.Messages;
using MajaMobile.Pages;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile
{
    public partial class MainPage : ContentPageBase
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MainPageViewModel();
            MessagingCenter.Subscribe<MajaConversationMessageLink>(this, MajaConversationMessageLink.LinkTappedMessage, LinkTapped);
            MessagingCenter.Subscribe<MajaConversationMessageLocation>(this, MajaConversationMessageLocation.LocationTappedMessage, LocationTapped);
            MessagingCenter.Subscribe<MajaConversationMessageWeather>(this, MajaConversationMessageWeather.WeatherTappedMessage, WeatherTapped);
        }

        private async void WeatherTapped(MajaConversationMessageWeather message)
        {
            if (!ViewModel.IsBusy)
                await Navigation.PushAsync(new WeatherPage(message.Weather));
        }

        private async void LocationTapped(MajaConversationMessageLocation message)
        {
            if (ViewModel.IsBusy)
                return;
            try
            {
                await Plugin.Share.CrossShare.Current.OpenBrowser(message.GetMapUrl(), new Plugin.Share.Abstractions.BrowserOptions() { ChromeToolbarColor = ColorScheme.UserMessageColor.ToShareColor() });
            }
            catch (Exception) { }
        }

        private async void LinkTapped(MajaConversationMessageLink message)
        {
            if (ViewModel.IsBusy)
                return;
            try
            {
                await Plugin.Share.CrossShare.Current.OpenBrowser(message.MajaQueryAnswer.Url, new Plugin.Share.Abstractions.BrowserOptions() { ChromeToolbarColor = ColorScheme.UserMessageColor.ToShareColor() });
            }
            catch (Exception) { }
        }

        public void ShiftEntryUp(double keyboardHeight)
        {
            if (MessageEntry.IsVisible)
            {
                MessageEntry.TranslationY = keyboardHeight * -1;
            }
            ActionButton.TranslationY = keyboardHeight * -1;
        }

        public void ShiftEntryDown()
        {
            if (MessageEntry.TranslationY != 0)
            {
                MessageEntry.TranslationY = 0;
            }
            ActionButton.TranslationY = 0;
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

        IAudioService _audioService;
        IDeviceInfo _deviceInfo;

        private const string MajaApiKey = "TODO: APIKEY";
        private const string MajaApiSecret = "TODO: APISECRET";
        private IOpenBiSession _openbiSession;
        private static IOpenBiConfiguration _openBiConfiguration = new OpenBiConfiguration(Protocol.HTTPS, "maja.ai", 443, "Maja UWP");
        private MajaConversationMessageThinking _thinkingMessage;
        private UserConversationMessage _speechRecognitionMessage;
        private bool _dialogActive;
        public IPossibleUserReply CurrentUserInput
        {
            get => GetField<IPossibleUserReply>();
            private set { SetField(value); OnPropertyChanged(); }
        }
        public ObservableCollection<ConversationMessage> Messages { get; } = new ObservableCollection<ConversationMessage>();

        public string Text
        {
            get => GetField<string>();
            set
            {
                SetField(value);
                UpdateChatButton();
                UpdateEntityInfo();
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
            _openbiSession = new OpenBiSession(_openBiConfiguration);

            CurrentMajaState = MajaListeningStatus.Idle;
            ChatButtonMode = ChatButtonDisplayMode.Microphone;
            LongPressedCommand = new Command(LongPressed);
            ReleasedCommand = new Command(Released);
            SendTextCommand = new Command(SendTextCommandExecuted);

            _audioService.CompletedAudio += _audioService_CompletedAudio;
            _audioService.StartedAudio += _audioService_StartedAudio;
            _audioService.SpeechRecognitionPartialResult += _audioService_SpeechRecognitionPartialResult;
            _audioService.SpeechRecognitionResult += _audioService_SpeechRecognitionResult;

            MessagingCenter.Subscribe<UserConversationMessageMultipleChoice, IPossibleUserReply>(this, UserConversationMessageMultipleChoice.MultipleChoiceTappedMessage, MultipleChoiceMessageTapped);
        }

        private void SendTextCommandExecuted(object parameter)
        {
            if (parameter is DateTime date)
            {
                var value = date.ToString(BiExcellence.OpenBi.Api.Internal.Utils.DateTimeOffsetToString(new DateTimeOffset(date)));
                var text = date.ToString("D");
                SendText(value, text);
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
                SendText(value, text, false);
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
                _thinkingMessage.StartThinking();
            }
            else
            {
                var message = _thinkingMessage;
                _thinkingMessage = null;
                if (message != null)
                {
                    message.StopThinking();
                    Messages.Remove(message);
                }
            }
            UpdateChatButton();
        }

        private void MultipleChoiceMessageTapped(UserConversationMessageMultipleChoice message, IPossibleUserReply possibleUserReply)
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
            SendText(possibleUserReply.Value, possibleUserReply.Text);
        }

        public ObservableCollection<IEntity> EntitySearchResults { get; } = new ObservableCollection<IEntity>();
        private string _lastSearch = "";
        private CancellationTokenSource _previousCts;
        private async void UpdateEntityInfo()
        {
            var possibleUserReply = CurrentUserInput;
            if (possibleUserReply == null || !string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase))
                return;
            var text = Text;
            if (text.Length >= 3 && !(_lastSearch == text))
            {
                _lastSearch = text;
                try
                {
                    CancelRunningTask();

                    var cts = _previousCts = new CancellationTokenSource();

                    var values = new Dictionary<string, string>();
                    values.Add("cms_item", "openbi:dblist");
                    values.Add("data-columns", "NAME;ID");
                    values.Add("data-items-per-page", "20");
                    values.Add("data-table", "SHOP_PRODUCT_" + possibleUserReply.Value);

                    var query = "NAME^=" + text;

                    using (var content = new FormUrlEncodedContent(values))
                    using (var response = await SendApiRequest(content, query, cts.Token))
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            _previousCts = null;
                            string result = await response.Content.ReadAsStringAsync();
                            var jarray = JArray.Parse(result);

                            EntitySearchResults.Clear();
                            var searchresults = new List<IEntity>();
                            foreach (var token in jarray)
                            {
                                var entity = new Entity((string)token["ID"]);
                                entity.Name = (string)token["NAME"];
                                searchresults.Add(entity);
                            }
                            foreach (var entity in searchresults.OrderBy(e => e.Name))
                            {
                                EntitySearchResults.Add(entity);
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private static async Task<HttpResponseMessage> SendApiRequest(HttpContent content, string urlParameters, CancellationToken token = default(CancellationToken))
        {
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                return await client.PostAsync("https://maja.ai?" + urlParameters, content, token);
            }
        }

        private void CancelRunningTask()
        {
            try
            {
                var previousCts = _previousCts;
                if (previousCts != null)
                {
                    try
                    {
                        previousCts.Cancel();
                    }
                    catch { }
                }
                _previousCts = null;
            }
            catch (Exception) { }
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

        private async void SendText(string value = null, string text = null, bool addMessage = true)
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
                else if (_dialogActive)
                {
                    foreach (var message in Messages.ToList())
                    {
                        if (message is UserConversationMessageMultipleChoice)
                        {
                            Messages.Remove(message);
                        }
                    }
                }
                if (addMessage)
                    Messages.Add(new UserConversationMessage(text ?? value));
                CurrentMajaState = MajaListeningStatus.Thinking;
                Text = "";
                try
                {
                    IList<IMajaQueryAnswer> answers = null;
                    try
                    {
                        //TODO:CancellationToken
                        answers = await _openbiSession.QueryMajaForAnswers(value, MajaApiKey, MajaApiSecret);
                    }
                    catch (OpenBiServerErrorException requestEx) when (requestEx.Response.Code == -97)
                    {
                        _openbiSession = new OpenBiSession(_openBiConfiguration);
                        //TODO:CancellationToken
                        answers = await _openbiSession.QueryMajaForAnswers(value, MajaApiKey, MajaApiSecret);
                    }
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
                            if (!string.IsNullOrEmpty(answer.Image))
                            {
                                Messages.Add(new MajaConversationMessageImage(answer));
                            }
                            List<IPossibleUserReply> possibleUserReplies = null;
                            foreach (var possibleUserReply in answer.PossibleUserReplies)
                            {
                                if (string.Equals(possibleUserReply.ControlType, PossibleUserReplyControlType.Button, StringComparison.OrdinalIgnoreCase) || (!(string.IsNullOrEmpty(possibleUserReply.Value)) && !string.Equals(possibleUserReply.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (possibleUserReplies == null)
                                        possibleUserReplies = new List<IPossibleUserReply>();
                                    possibleUserReplies.Add(possibleUserReply);
                                }
                                else
                                {
                                    CurrentUserInput = possibleUserReply;
                                }
                            }
                            if (possibleUserReplies != null)
                                Messages.Add(new UserConversationMessageMultipleChoice("", possibleUserReplies));
                            completed = completed && answer.Completed;
                        }
                        _dialogActive = !completed;
                    }
                }
                catch (OperationCanceledException)
                {
                    //TODO
                    _dialogActive = false;
                }
                catch (Exception ex)
                {
                    _dialogActive = false;
                    Messages.Add(new MajaConversationMessage(ex.Message));
                    //TODO
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
                _deviceInfo.Vibrate();
            }
            else if (ChatButtonMode == ChatButtonDisplayMode.Send)
                SendText();
            Console.WriteLine("MainPage_Released");
        }
        
        private void LongPressed(object obj)
        {
            if (IsIdle && ChatButtonMode == ChatButtonDisplayMode.Microphone)
            {
                _audioService.StartSpeechRecognition();
                ChatButtonMode = ChatButtonDisplayMode.Listening;
                _deviceInfo.Vibrate();
            }
            Console.WriteLine("MainPage_LongPressed");
        }
    }
}