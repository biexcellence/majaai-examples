using BiExcellence.OpenBi.Api.Commands.MajaAi;
using GalaSoft.MvvmLight.Messaging;
using MajaUWP.Office;
using MajaUWP.Pages;
using MajaUWP.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace MajaUWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public SpeechRecognitionService _speechRecognitionService { get; set; }
        private SessionHandler _sessionHandler;
        public MajaConversation _majaConversation { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Utils.AddDefaultPackages();
            ApplicationView.PreferredLaunchViewSize = new Size(800, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(800, 600));
            RequestedTheme = ElementTheme.Light;

            if (_speechRecognitionService == null)
                _speechRecognitionService = new SpeechRecognitionService(AudioPlayer, this.BaseUri);
            else
                _speechRecognitionService.SetAudioPlayer(AudioPlayer);
            if (_sessionHandler == null)
                _sessionHandler = new SessionHandler();
            if (_majaConversation == null)
            {
                _majaConversation = new MajaConversation(_sessionHandler);
                _majaConversation.AnswersReceived += MajaQueryAnswersReceived;
            }
            ContentFrame.Navigate(typeof(ChatPage), new object[] { _majaConversation, _speechRecognitionService });

            SetupMessengers();
            

            AppSettingHandler.SetUpLogin();

        }

        private void SetupMessengers()
        {
            Messenger.Default.Register<(string, DateTimeOffset, string)>(this, (nm) =>
            {
                if (nm.Item1 == "alarm")
                {
                    setAlarm(nm.Item2, nm.Item3);
                }
            });

            Messenger.Default.Register<string>(this, (nm) =>
            {
                switch (nm)
                {
                    case "playStartDing":
                        PlayAudio(new Uri("ms-appx:///Assets/Ding_start.mp3"));
                        break;
                    case "playStopDing":
                        PlayAudio(new Uri("ms-appx:///Assets/Ding_end.mp3"));
                        break;
                    case "openSettingsPage":
                        ContentFrame.Navigate(typeof(SettingsPage), _majaConversation);
                        break;
                    case "openTalentPickerPage":
                        ContentFrame.Navigate(typeof(TalentPickerPage));
                        break;
                    default:
                        break;
                }
            }
            );
        }

        private void PlayAudio(Uri toPlay)
        {
            MediaPlayer player = new MediaPlayer();
            player.Source = MediaSource.CreateFromUri(toPlay);
            player.Volume = (double) AppSettingHandler.GetAppSetting("alarmVolume");
            player.Play();
            player.MediaEnded += (s, o) => {
                MediaPlayer mp = s as MediaPlayer;
                s.Dispose();
            };
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            _speechRecognitionService.Dispose();
            if (_majaConversation != null)
            {
                _majaConversation.AnswersReceived -= MajaQueryAnswersReceived;
            }
        }

        private async void MajaQueryAnswersReceived(object sender, MajaQueryEventArgs e)
        {
            bool replyExcpected = false;
            if (e.Answers != null && e.Answers.Count > 0)
            {
                var answer = e.Answers.First();
                switch (answer.ProposalType)
                {
                    case MajaQueryAnswerProposalType.AudioFile:
                        await _speechRecognitionService.PlayAudio(answer.Action);
                        break;
                    case MajaQueryAnswerProposalType.VideoFile:
                        if (!string.IsNullOrEmpty(answer.Action))
                        {
                            ContentFrame.Navigate(typeof(VideoPage), answer.Action);
                            return;
                        }
                        break;
                    case MajaQueryAnswerProposalType.Location when answer.Entities.Count > 0:
                        var dict = new Dictionary<string, List<string>>();

                        SetupLocationPage(answer, dict);
                        if (dict.Count > 0)
                        {
                            var url = new Uri("https://maps.maja.ai/?" + string.Join("&", dict.Select(kv => string.Join("&", kv.Value.Select(v => kv.Key + "=" + Uri.EscapeDataString(v))))));
                            ContentFrame.Navigate(typeof(BrowserPage), url);
                            return;
                        }
                        break;
                    case MajaQueryAnswerProposalType.ImmoSuche:
                        ContentFrame.Navigate(typeof(ImmoPage), answer, new DrillInNavigationTransitionInfo());
                        return;
                    case MajaQueryAnswerProposalType.Link when !string.IsNullOrEmpty(answer.Url):
                        try
                        {
                            var uri = new Uri(answer.Url);
                            if (string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) || string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                                ContentFrame.Navigate(typeof(BrowserPage), uri);
                            else
                                await Launcher.LaunchUriAsync(uri);
                            return;
                        }
                        catch { }
                        break;
                    default:
                        //bool dateResponseRequested = false;
                        //foreach (var userReply in answer.PossibleUserReplies)
                        //{
                        //    if (userReply.Type == "DATE")
                        //    {
                        //        dateResponseRequested = true;
                        //    }
                        //}
                        //if (dateResponseRequested)
                        //{
                        //    ContentFrame.Navigate(typeof(DatePickerPage), _majaConversation);
                        //}
                        if (string.Equals(answer.Action, MajaQueryAnswerAction.Weather, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(answer.Data))
                        {
                            ContentFrame.Navigate(typeof(WeatherPage), new WeatherForecast(answer.Data));
                            return;
                        }
                        OpenCorrespondingPageIfNeccessary(answer);
                        break;
                }
                if (answer.PossibleUserReplies.Count > 0 && string.IsNullOrEmpty(answer.Data)) replyExcpected = true;
                

                
            }
            if (!string.IsNullOrEmpty(e.SpeakingText))
            {
                _majaConversation.MajaStatus = MajaListeningStatus.Speaking;
                await _speechRecognitionService.SpeakTextAsync(e.SpeakingText);
                _majaConversation.MajaStatus = MajaListeningStatus.Idle;
                if (replyExcpected)
                {
                    
                }
                
            }
        }

        private async void OpenCorrespondingPageIfNeccessary(IMajaQueryAnswer answer)
        {
            if (answer.Data != null && answer.Data.StartsWith("Calendar"))
            {
                ContentFrame.Navigate(typeof(CalendarPage), answer.Data);
            }
            if (answer.Data != null && answer.Data.StartsWith("Mail"))
            {
                ContentFrame.Navigate(typeof(MailPage), answer.Data);
            }
            if (answer.Data != null && answer.Data.StartsWith("Contacts"))
            {
                ContentFrame.Navigate(typeof(ContactPickerPage), (_majaConversation, answer.Data));
            }
            if (answer.Data == "returnToken" && string.IsNullOrEmpty(Utils.microSoftToken))
            {
                ContentFrame.Navigate(typeof(MicrosoftLoginPage), _majaConversation);
            }
            if (answer.Data != null && answer.Data == "returnDateTime")
            {
                ContentFrame.Navigate(typeof(DateTimePickerPage), _majaConversation);
            }
            if (answer.Data != null && answer.Data == "setAlarm")
            {
                ContentFrame.Navigate(typeof(Alarm_Page), (_majaConversation, true));
            }
            if (answer.Data != null && answer.Data == "setTimer")
            {
                ContentFrame.Navigate(typeof(Alarm_Page), (_majaConversation, false));
            }
            if (answer.Data != null && answer.Data.StartsWith("writeTodo") )
            {
                TodoHandler tdh = new TodoHandler();
                await tdh.AddToSavedList(new TodoItem(answer.Data.Substring(9),TodoItem.UrgencyStates.normalPriority));
                ContentFrame.Navigate(typeof(ToDoListPage));
            }
            if (answer.Data != null && answer.Data == "showTodo")
            {
                ContentFrame.Navigate(typeof(ToDoListPage));
            }
        }

        private static void SetupLocationPage(IMajaQueryAnswer answer, Dictionary<string, List<string>> dict)
        {
            foreach (var entity in answer.Entities)
            {
                if (entity.DisplayAttributes.TryGetValue("lat", out var lat) && entity.DisplayAttributes.TryGetValue("lon", out var @long))
                {
                    if (!dict.TryGetValue("lat", out var lats))
                    {
                        lats = new List<string>();
                        dict["lat"] = lats;
                    }
                    lats.Add(((double)lat).ToString(CultureInfo.InvariantCulture));

                    if (!dict.TryGetValue("lng", out var lngs))
                    {
                        lngs = new List<string>();
                        dict["lng"] = lngs;
                    }
                    lngs.Add(((double)@long).ToString(CultureInfo.InvariantCulture));


                    if (!dict.TryGetValue("name", out var names))
                    {
                        names = new List<string>();
                        dict["name"] = names;
                    }
                    names.Add(entity.Name);
                }
            }
        }

        public void setAlarm(DateTimeOffset toSetTo, string message) {
            var now = DateTimeOffset.Now;
            
            var alarmInMs = toSetTo.Subtract(now).TotalMilliseconds;
            DispatcherTimer alarmTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(alarmInMs) };
            alarmTimer.Tick += async (s, e) => {
                alarmTimer.Stop();

                PlayAudio(new Uri("ms-appx:///Assets/alarmTone.mp3"));

                var dialog = new MessageDialog("Timer abgelaufen!!");
                var result = await dialog.ShowAsync();               
            };
            alarmTimer.Start();
        }
        private async void ShowMessage(string message)
        {
            //TODO: remove when in release mode? do we want to show error messages?
            if (Dispatcher.HasThreadAccess)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                await messageDialog.ShowAsync();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                    await messageDialog.ShowAsync();
                });
            }
        }

        private async void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is TextBox txt && e.Key == VirtualKey.Enter && !string.IsNullOrEmpty(txt.Text))
            {
                var text = txt.Text;
                txt.Text = "";
                await _majaConversation.QueryMajaForAnswers(text);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (!(ContentFrame.Content is ChatPage))
                ContentFrame.GoBack();
        }
    }
}

namespace MajaUWP.Converters
{

    public class MajaConversationMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var speaker = (MajaConversationSpeaker)value;
            switch (speaker)
            {
                case MajaConversationSpeaker.User when targetType == typeof(Brush):
                    return "#6797bf";
                case MajaConversationSpeaker.Maja when targetType == typeof(Brush):
                    return "#ededed";
                case MajaConversationSpeaker.Maja when targetType == typeof(Thickness):
                    return new Thickness(0, 10, 100, 10);
                case MajaConversationSpeaker.User when targetType == typeof(Thickness):
                    return new Thickness(100, 10, 0, 10);
                case MajaConversationSpeaker.Maja:
                    return HorizontalAlignment.Left;
                case MajaConversationSpeaker.User:
                    return HorizontalAlignment.Right;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var speaker = (MajaConversationSpeaker)value;

            switch (speaker)
            {
                case MajaConversationSpeaker.Maja:
                    return "#000000";
                case MajaConversationSpeaker.User:
                    return "#ffffff";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PageContentToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value is ChatPage)
                if (parameter == null)
                    return Visibility.Collapsed;
                else
               if (Package.Current.Id.Architecture == ProcessorArchitecture.Arm)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            if (parameter == null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MajaStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (MajaListeningStatus)value;
            switch (status)
            {
                case MajaListeningStatus.Unknown:
                    return "../Assets/maja_no_bg.png";
                case MajaListeningStatus.Idle:
                    return "../Assets/maja_no_bg.png";
                case MajaListeningStatus.Thinking:
                    return "../Assets/maja_no_bg.png";
                case MajaListeningStatus.Speaking:
                    return "../Assets/Sprachausgabe.png";
                case MajaListeningStatus.Listening:
                    return "../Assets/Spracheingabe.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}