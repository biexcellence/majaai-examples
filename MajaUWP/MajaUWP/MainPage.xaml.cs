using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Pages;
using MajaUWP.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Core;
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
        private SpeechRecognitionService _speechRecognitionService;
        private SessionHandler _sessionHandler;
        private MajaConversation _majaConversation;

        public MainPage()
        {
            InitializeComponent();
            if (_speechRecognitionService == null)
                _speechRecognitionService = new SpeechRecognitionService(AudioPlayer);
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
            if (e.Answers != null && e.Answers.Count > 0)
            {
                var answer = e.Answers.First();
                switch (answer.ProposalType)
                {
                    case MajaQueryAnswerProposalType.AudioFile:
                        _speechRecognitionService.PlayAudio(answer.Action);
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
                        if (string.Equals(answer.Action, MajaQueryAnswerAction.Weather, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(answer.Data))
                        {
                            ContentFrame.Navigate(typeof(WeatherPage), new WeatherForecast(answer.Data));
                            return;
                        }
                        break;
                }
            }
            if (!string.IsNullOrEmpty(e.SpeakingText))
                await _speechRecognitionService.SpeakTextAsync(e.SpeakingText);
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
                case MajaConversationSpeaker.Maja when targetType == typeof(Brush):
                    return new SolidColorBrush(Windows.UI.Colors.LightGreen);
                case MajaConversationSpeaker.User when targetType == typeof(Brush):
                    return new SolidColorBrush(Windows.UI.Colors.LightGray);
                case MajaConversationSpeaker.Maja when targetType == typeof(Thickness):
                    return new Thickness(0, 5, 100, 5);
                case MajaConversationSpeaker.User when targetType == typeof(Thickness):
                    return new Thickness(100, 5, 0, 5);
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
}