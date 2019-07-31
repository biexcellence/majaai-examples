using BiExcellence.OpenBi.Api.Commands.MajaAi;
using GalaSoft.MvvmLight.Messaging;
using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Input;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private ChatPageViewModel _viewModel;
        CancellationTokenSource cts = new CancellationTokenSource();

        public ChatPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is object[] arr && arr.Length >= 2 && arr[0] is MajaConversation conversation && arr[1] is SpeechRecognitionService service)
            {
                DataContext = _viewModel = new ChatPageViewModel(conversation, service);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            _viewModel.Dispose();
        }

        private async void Maja_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (_viewModel.MajaConversation.MajaStatus)
            {
                case MajaListeningStatus.Idle:
                    _viewModel.StartListening();
                    break;
                case MajaListeningStatus.Listening:
                case MajaListeningStatus.Speaking:
                    _viewModel.MajaConversation.StopListening();
                    await _viewModel.SpeechRecognitionService.Stop();
                    _viewModel.MajaConversation.MajaStatus = MajaListeningStatus.Idle;
                    break;
            }
        }

        private void ChatScrollBox_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer) {
               //scroll(scrollViewer);
            }

        }

        private void scroll(ScrollViewer toscroll) {
            toscroll.UpdateLayout();
            toscroll.ChangeView(null, int.MaxValue, null);
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebView webView && !string.IsNullOrEmpty( _viewModel.MajaConversation.Messages[_viewModel.MajaConversation.Messages.Count-1].Html))
            {
                string html = "<!DOCTYPE html><html><head><style>@font-face { font-family: 'segoeui';  src: url('segoeui.ttf'); } html, body {margin: 0; padding: 0; font-size: 20px; background-color: #ededed; font-family: 'segoeui'; color: #000000;} .majaai-reply { /*ios hack...*/ cursor: pointer; }</style></head><body>" +

                                _viewModel.MajaConversation.Messages[_viewModel.MajaConversation.Messages.Count - 1].Html +
                                @"<script>document.body.addEventListener(""click"", function(e) { if (e.target.classList.contains(""majaai-reply"")) { var value = e.target.dataset[""value""] || e.target.value || e.target.textContent; location.href = ""http://localhost/clicked?value="" + encodeURIComponent(value); }  }, false);
                                </script>" +
                                "</body></html>";


                webView.NavigateToString(html);
                
            }
            scroll(ChatScrollBox);
        }

        private async void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null && args.Uri.AbsoluteUri.StartsWith("http://localhost/clicked?value="))
            {
                args.Cancel = true;
                await _viewModel.MajaConversation.QueryMajaForAnswers(args.Uri.AbsoluteUri.Remove(0, 31),null,false,cts.Token);
            }

        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            WebView webView = sender as WebView;

            string height = await webView.InvokeScriptAsync("eval", new string[] { "document.body.scrollHeight.toString()" });
            sender.Height = int.Parse(height);
            scroll(ChatScrollBox);
        }

        private void Button_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            scroll(ChatScrollBox);
        }

        private async void FilePickerButton_Clicked(object sender, RoutedEventArgs e)
        {

            var deviceInfo = AnalyticsInfo.VersionInfo.DeviceFamily;


            if (deviceInfo != "Windows.IoT")
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                picker.FileTypeFilter.Add("*");




                StorageFile file = await picker.PickSingleFileAsync();






                try
                {
                    if (file != null)
                    {
                        var copiedFile = await file.CopyAsync(ApplicationData.Current.LocalFolder);

                        FileStream fs = File.Open(copiedFile.Path, FileMode.Open, FileAccess.ReadWrite);





                        try
                        {
                            var fileUrl = await _viewModel.MajaConversation.SessionHandler.ExecuteOpenbiCommand((s, t) => s.UploadMajaUserFile(fs, file.DisplayName, t));
                            _viewModel.MajaConversation.Messages.Add(new MajaConversationMessage("Datei Erfolgreich Hochgeladen"));
                            await _viewModel.MajaConversation.SessionHandler.ExecuteOpenbiCommand((s, t) => s.QueryMajaForAnswers(fileUrl, Utils.MajaApiKey, Utils.MajaApiSecret, t));

                        }
                        catch (Exception ex)
                        {

                            _viewModel.MajaConversation.Messages.Add(new MajaConversationMessage(ex.Message));
                        }
                        await copiedFile.DeleteAsync();

                    }
                    else
                    {
                        _viewModel.MajaConversation.Messages.Add(new MajaConversationMessage("Keine Datei ausgewählt! Diese Feature ist nicht auf dem Raspberry Pi verfügbar"));
                    }



                }
                catch (Exception filePickerException)
                {

                    _viewModel.MajaConversation.Messages.Add(new MajaConversationMessage(filePickerException.Message));
                }
            }
            else
            {
                _viewModel.MajaConversation.Messages.Add(new MajaConversationMessage("Dieses Feature wird auf dem Gerät leider nicht Unterstützt"));
            }
        }

        private async void DatePickerFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
        {
            DatePickerFlyout picker = sender as DatePickerFlyout;
            string dateString = "";

            DateTimeOffset date = picker.Date;

            string day = date.Day.ToString();
            if (day.Length == 1) day = "0" + day;

            string month = date.Month.ToString();
            if (month.Length == 1) month = "0" + month;

            dateString = day + "." + month + "." + date.Year.ToString();

            await _viewModel.MajaConversation.QueryMajaForAnswers(dateString);
            
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class ChatPageViewModel : ViewModelBase
    {
        /// <summary>
        /// This HResult represents the scenario where a user is prompted to allow in-app speech, but 
        /// declines. This should only happen on a Phone device, where speech is enabled for the entire device,
        /// not per-app.
        /// </summary>
        private static uint HResultPrivacyStatementDeclined = 0x80045509;

        public SpeechRecognitionService SpeechRecognitionService { get; }
        public MajaConversation MajaConversation { get; }
        public ICommand UserReplyCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand OpenDatePageCommand { get; }
        public ICommand SettingsCommand { get; }
        public string DateTimeString { get;
            set; }

        public bool IsLoggedIn => !String.IsNullOrEmpty(Utils.microSoftToken);


        public ChatPageViewModel(MajaConversation majaConversation, SpeechRecognitionService service)
        {
            MajaConversation = majaConversation;
            SpeechRecognitionService = service;
            UserReplyCommand = new Command(SendUserReply);
            StopCommand = new Command(async () => {
                MajaConversation.cts.Cancel();
                Messenger.Default.Send<string>("playStopDing");
                await MajaConversation.QueryMajaForAnswers("stop"); });
            LogoutCommand = new Command(() => {
                Utils.microSoftToken = "";
                MajaConversation.Messages.Add(new ConversationMessage("Erfolgreich abgemeldet", MajaConversationSpeaker.Maja));
                OnPropertyChanged(nameof(IsLoggedIn));});
            SettingsCommand = new Command(() => {
                Messenger.Default.Send<string>("openSettingsPage");
            });
            service.HypothesisGenerated += Service_HypothesisGenerated;
            SetupDateTime();
        }

        private void SetupDateTime()
        {
            //get current time
            DateTime localDateTime;
            //localDateTime = getCurrentInternetTime();
            localDateTime = DateTime.Now;

            DateTimeString = localDateTime.ToString("HH:mm \r\n dd. MMM yyyy");
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            timer.Tick += (s, e) => {
                //localDateTime = getCurrentInternetTime();
                localDateTime = DateTime.Now;
                DateTimeString  = localDateTime.ToString("HH:mm \r\n dd. MMM yyyy");
                OnPropertyChanged(nameof(DateTimeString)); };
            timer.Start();
        }

        private static DateTime getCurrentInternetTime()
        {
            DateTime localDateTime;
            var client = new TcpClient("time.nist.gov", 13);
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                try
                {
                    var response = streamReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(response))
                    {
                        var utcDateTimeString = response.Substring(7, 17);
                        localDateTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    }
                    else
                    {
                        return DateTime.Now;
                    }
                    
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
                
            }

            return localDateTime;
        }

        public override void Dispose()
        {
            base.Dispose();
            SpeechRecognitionService.HypothesisGenerated -= Service_HypothesisGenerated;
        }

        private void Service_HypothesisGenerated(object sender, SpeechRecognitionHypothesisGeneratedEventArgs e)
        {
            MajaConversation.SetUserText(e.Hypothesis.Text.ToLower());
        }

        private async void SendUserReply(object obj)
        {
            if (obj is UserReply possibleUserReply)
            {
                await SpeechRecognitionService.Stop();
                await MajaConversation.QueryMajaForAnswers(possibleUserReply.Value, possibleUserReply.Text);
            }
        }

        public async void StartListening()
        {
            try
            {
                MajaConversation.MajaStatus = MajaListeningStatus.Listening;
                var result = await SpeechRecognitionService.RecognizeAsync();
                if (result == null)
                {
                    Messenger.Default.Send<string>("playStopDing");
                    return;
                }
                if (result.Confidence == SpeechRecognitionConfidence.Medium || result.Confidence == SpeechRecognitionConfidence.High)
                {
                    await MajaConversation.QueryMajaForAnswers(result.Text, addMessage: false);
                }
                else if (!string.IsNullOrEmpty(result.Text))
                {
                    Messenger.Default.Send<string>("playStopDing");
                    MajaConversation.Messages.Add(new MajaConversationMessage("Das habe ich akustisch nicht verstanden, bitte sprich direkt in das Mikrofon"));
                }
                else
                {
                    Messenger.Default.Send<string>("playStopDing");
                }
            }
            catch (OperationCanceledException)
            {
                Messenger.Default.Send<string>("playStopDing");
                //_speechCancellationTokenSource = null;
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>("playStopDing");
                //_speechCancellationTokenSource = null;
                if ((uint)ex.HResult == HResultPrivacyStatementDeclined)
                {
                    // Show a UI link to the privacy settings.
                    SpeechRecognitionService.ShowMessage("Please restart the program, permission of microphone denied");
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping"));
                }
                else
                {
                    MajaConversation.Messages.Add(new MajaConversationMessage("Es kam beim Zugriff auf das Mikrofon zu einem Fehler. Bitte versuche es erneut."));
                }
            }
            if (MajaConversation.MajaStatus != MajaListeningStatus.Speaking)
                MajaConversation.MajaStatus = MajaListeningStatus.Idle;
        }
    }
}