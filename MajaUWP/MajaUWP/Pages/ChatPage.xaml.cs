using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using System;
using System.Windows.Input;
using Windows.Media.SpeechRecognition;
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
                case MajaListeningStatus.Thinking:
                    _viewModel.MajaConversation.StopListening();
                    await _viewModel.SpeechRecognitionService.Stop();
                    _viewModel.MajaConversation.MajaStatus = MajaListeningStatus.Idle;
                    break;
            }
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

        public ChatPageViewModel(MajaConversation majaConversation, SpeechRecognitionService service)
        {
            MajaConversation = majaConversation;
            SpeechRecognitionService = service;
            UserReplyCommand = new Command(SendUserReply);
            service.HypothesisGenerated += Service_HypothesisGenerated;
        }

        public override void Dispose()
        {
            base.Dispose();
            SpeechRecognitionService.HypothesisGenerated -= Service_HypothesisGenerated;
        }

        private void Service_HypothesisGenerated(object sender, Windows.Media.SpeechRecognition.SpeechRecognitionHypothesisGeneratedEventArgs e)
        {
            MajaConversation.SetUserText(e.Hypothesis.Text.ToLower());
        }

        private async void SendUserReply(object obj)
        {
            if (obj is IPossibleUserReply possibleUserReply)
            {
                await MajaConversation.QueryMajaForAnswers(possibleUserReply.Value, possibleUserReply.Text);
            }
        }

        public async void StartListening()
        {
            //TODO: cancel option with cancellationtoken
            MajaConversation.MajaStatus = MajaListeningStatus.Listening;
            try
            {
                MajaConversation.MajaStatus = MajaListeningStatus.Listening;
                var result = await SpeechRecognitionService.RecognizeAsync();
                if (result.Confidence == SpeechRecognitionConfidence.Medium || result.Confidence == SpeechRecognitionConfidence.High)
                {
                    await MajaConversation.QueryMajaForAnswers(result.Text, addMessage: false);
                }
                else if (!string.IsNullOrEmpty(result.Text))
                {
                    MajaConversation.Messages.Add(new MajaConversationMessage("Das habe ich akustisch nicht verstanden, bitte sprich direkt in das Mikrofon"));
                }
            }
            catch (OperationCanceledException)
            {
                //_speechCancellationTokenSource = null;
            }
            catch (Exception ex)
            {
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
            MajaConversation.MajaStatus = MajaListeningStatus.Idle;
        }
    }
}