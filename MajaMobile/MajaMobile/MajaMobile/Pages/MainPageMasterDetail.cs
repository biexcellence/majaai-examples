using BiExcellence.OpenBi.Api.Commands.Users;
using MajaMobile.Messages;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public class MainPageMasterDetail : MasterDetailPage
    {
        private MainPageMasterViewModel _viewModel;
        private MainPage _mainPage;

        public MainPageMasterDetail()
        {
            var binding = new Binding(nameof(MainPageMasterViewModel.IsPresented), BindingMode.TwoWay);
            SetBinding(IsPresentedProperty, binding);
            BindingContext = _viewModel = new MainPageMasterViewModel();

            Master = new MainPageMaster();

            _mainPage = new MainPage();
            var navigationPage = new NavigationPageBase(_mainPage);//{ Icon = "Icon-Small.png" };
            Detail = navigationPage;
            navigationPage.Popped += NavigationPage_Popped;
            navigationPage.Pushed += NavigationPage_Pushed;
        }

        private void NavigationPage_Pushed(object sender, NavigationEventArgs e)
        {
            _viewModel.IsPresented = false;
            if (!(e.Page is MainPage))
            {
                IsGestureEnabled = false;
            }
        }

        private void NavigationPage_Popped(object sender, NavigationEventArgs e)
        {
            IsGestureEnabled = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<MajaConversationMessageLink>(this, ConversationMessage.ConversationMessageTappedMessage, LinkTapped);
            MessagingCenter.Subscribe<MajaConversationMessageLocation>(this, ConversationMessage.ConversationMessageTappedMessage, LocationTapped);
            MessagingCenter.Subscribe<MajaConversationMessageWeather>(this, ConversationMessage.ConversationMessageTappedMessage, WeatherTapped);
            MessagingCenter.Subscribe<MajaConversationMessageImmo>(this, ConversationMessage.ConversationMessageTappedMessage, ImmoTapped);
            MessagingCenter.Subscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.RegisterMessage, Register);
            MessagingCenter.Subscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.SelectTalentsMessage, SelectTalents);
            MessagingCenter.Subscribe(this, MainPageMasterViewModel.LoginMessage, async (MainPageMasterViewModel viewmodel) =>
            {
                if (CanNavigate())
                    await Detail.Navigation.PushAsync(new LoginPage());
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<MajaConversationMessageLink>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MajaConversationMessageLocation>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MajaConversationMessageWeather>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MajaConversationMessageImmo>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.RegisterMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.SelectTalentsMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.LoginMessage);
        }

        private void Register(MainPageMasterViewModel viewmodel)
        {
            //TODO
        }

        private void SelectTalents(MainPageMasterViewModel viewmodel)
        {
            //TODO
        }

        private async void ImmoTapped(MajaConversationMessageImmo message)
        {
            if (CanNavigate())
            {
                await Detail.Navigation.PushAsync(new ImmoPage(message));
            }
        }

        private async void WeatherTapped(MajaConversationMessageWeather message)
        {
            if (CanNavigate())
            {
                await Detail.Navigation.PushAsync(new WeatherPage(message.Weather));
            }
        }

        private async void LocationTapped(MajaConversationMessageLocation message)
        {
            if (CanNavigate())
            {
                try
                {
                    var location = message.GetLocation();
                    await Map.OpenAsync(location.Lat, location.Long);
                }
                catch (Exception) { }
            }
        }

        private async void LinkTapped(MajaConversationMessageLink message)
        {
            if (CanNavigate())
            {
                try
                {
                    await Browser.OpenAsync(message.MajaQueryAnswer.Url, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception) { }
            }
        }

        //Prevent double tap
        private bool CanNavigate()
        {
            var page = Detail.Navigation.NavigationStack.Last();
            return page is MainPage mainPage && mainPage.IsIdle;
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class MainPageMasterViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SelectTalentsCommand { get; }
        public const string RegisterMessage = "REGISTER";
        public const string LoginMessage = "LOGIN";
        public const string SelectTalentsMessage = "SELECT_TALENTS";

        public bool IsPresented
        {
            get => GetField<bool>();
            set => SetField(value);
        }

        public IUser User
        {
            get => GetField<IUser>();
            set { SetField(value); }
        }

        public MainPageMasterViewModel()
        {
            LoginCommand = new Command(() => MessagingCenter.Send(this, LoginMessage));
            RegisterCommand = new Command(() => MessagingCenter.Send(this, RegisterMessage));
            SelectTalentsCommand = new Command(() => MessagingCenter.Send(this, SelectTalentsMessage));
            LogoutCommand = new Command(() => SessionHandler.Instance.Logout());
            MessagingCenter.Subscribe(this, SessionHandler.UserChangedMessage, (SessionHandler s, IUser user) => User = user);
        }
    }
}