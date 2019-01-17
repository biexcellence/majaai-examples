using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using System.Windows.Input;
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
        ChatPageViewModel _viewModel;
        private SpeechRecognitionService _speechRecognitionService;
        public ChatPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (_speechRecognitionService == null && e.Parameter is SpeechRecognitionService service)
            {
                _speechRecognitionService = service;
                DataContext = _viewModel = new ChatPageViewModel(service.MajaConversation);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            _speechRecognitionService = null;
        }

        private async void Maja_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var timerEnabled = MainPage.IdleTimer.IsEnabled;
            MainPage.IdleTimer.Stop();
            switch (_speechRecognitionService.MajaConversation.MajaStatus)
            {
                case MajaListeningStatus.Idle:
                    _speechRecognitionService.StartSpeechRecognition();
                    break;
                case MajaListeningStatus.Listening:
                case MajaListeningStatus.Speaking:
                case MajaListeningStatus.Thinking:
                    await _speechRecognitionService.StopMaja();
                    break;
            }
            if (timerEnabled)
                MainPage.IdleTimer.Start();
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class ChatPageViewModel : ViewModelBase
    {

        public MajaConversation MajaConversation { get; }
        public ICommand UserReplyCommand { get; }

        public ChatPageViewModel(MajaConversation majaConversation)
        {
            MajaConversation = majaConversation;
            UserReplyCommand = new Command(SendUserReply);
        }

        private async void SendUserReply(object obj)
        {
            if (obj is IPossibleUserReply possibleUserReply)
            {
                await MajaConversation.SendMajaQuery(possibleUserReply.Value, possibleUserReply.Text);
            }
        }
    }
}
