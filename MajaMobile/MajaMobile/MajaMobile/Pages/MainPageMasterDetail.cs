using BiExcellence.OpenBi.Api.Commands.Users;
using MajaMobile.Messages;
using MajaMobile.Models;
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
            var style = (Style)Application.Current.Resources["ContentPageStyle"];
            Style = style;
            MasterBehavior = MasterBehavior.Popover;

            var sessionHandler = new SessionHandler();

            var binding = new Binding(nameof(MainPageMasterViewModel.IsPresented), BindingMode.TwoWay);
            SetBinding(IsPresentedProperty, binding);
            BindingContext = _viewModel = new MainPageMasterViewModel(sessionHandler);

            Master = new MainPageMaster(_viewModel);

            _mainPage = new MainPage(sessionHandler);
            var navigationPage = new NavigationPageBase(_mainPage);//{ Icon = "Icon-Small.png" };
            Detail = navigationPage;
            navigationPage.Popped += NavigationPage_Popped;
            navigationPage.Pushed += NavigationPage_Pushed;
        }

        private void NavigationPage_Pushed(object sender, NavigationEventArgs e)
        {
            IsGestureEnabled = false;
        }

        private void NavigationPage_Popped(object sender, NavigationEventArgs e)
        {
            IsGestureEnabled = Detail.Navigation.NavigationStack.Count == 1;
            if(e.Page is ContentPageBase page)
            {
                page.Dispose();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<MajaConversationMessageLink>(this, ConversationMessage.ConversationMessageTappedMessage, LinkTapped);
            MessagingCenter.Subscribe<MajaConversationMessageLocation>(this, ConversationMessage.ConversationMessageTappedMessage, LocationTapped);
            MessagingCenter.Subscribe<MajaConversationMessageImmo>(this, ConversationMessage.ConversationMessageTappedMessage, ImmoMessageTapped);
            MessagingCenter.Subscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.RegisterMessage, Register);
            MessagingCenter.Subscribe(this, MainPageMasterViewModel.SelectTalentsMessage, async (MainPageMasterViewModel viewmodel) =>
            {
                if (PrepareNavigation())
                    await Detail.Navigation.PushAsync(new TalentsPage(_viewModel.SessionHandler));
            });
            MessagingCenter.Subscribe(this, MainPageMasterViewModel.LoginMessage, async (MainPageMasterViewModel viewmodel) =>
            {
                if (PrepareNavigation())
                    await Detail.Navigation.PushAsync(new LoginPage(_viewModel.SessionHandler));
            });
            MessagingCenter.Subscribe(this, MainPageMasterViewModel.EditUserProfileMessage, async (MainPageMasterViewModel viewmodel, IUser user) =>
            {
                if (user != null && PrepareNavigation())
                {
                    await Detail.Navigation.PushAsync(new UserProfilePage(user, _viewModel.SessionHandler));
                }
            });
            MessagingCenter.Subscribe<ImmoObject>(this, ImmoObject.TappedMessage, ImmoTapped);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<MajaConversationMessageLink>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MajaConversationMessageLocation>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MajaConversationMessageImmo>(this, ConversationMessage.ConversationMessageTappedMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.RegisterMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.SelectTalentsMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel>(this, MainPageMasterViewModel.LoginMessage);
            MessagingCenter.Unsubscribe<ImmoObject>(this, ImmoObject.TappedMessage);
            MessagingCenter.Unsubscribe<MainPageMasterViewModel, IUser>(this, MainPageMasterViewModel.EditUserProfileMessage);
        }

        private async void ImmoTapped(ImmoObject immo)
        {
            if (PrepareNavigation())
            {
                try
                {
                    await Browser.OpenAsync(immo.Link, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception) { }
            }
        }

        private void Register(MainPageMasterViewModel viewmodel)
        {
            //TODO
        }

        private async void ImmoMessageTapped(MajaConversationMessageImmo message)
        {
            if (PrepareNavigation())
            {
                await Detail.Navigation.PushAsync(new ImmoPage(message));
            }
        }

        private async void LocationTapped(MajaConversationMessageLocation message)
        {
            if (PrepareNavigation())
            {
                try
                {
                    var location = message.Locations.FirstOrDefault();
                    if (location != null)
                        await Map.OpenAsync(location.Lat, location.Long, new MapLaunchOptions() { Name = location.Name });
                }
                catch (Exception) { }
            }
        }

        private async void LinkTapped(MajaConversationMessageLink message)
        {
            if (PrepareNavigation())
            {
                try
                {
                    await Browser.OpenAsync(message.MajaQueryAnswer.Url, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception) { }
            }
        }

        //Prevent double tap
        private bool PrepareNavigation()
        {
            if (Detail.Navigation.NavigationStack.LastOrDefault() is MainPage page && page.IsIdle)
            {
                _viewModel.IsPresented = false;
                return true;
            }
            return false;
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
        public ICommand ProfileExpanderCommand { get; }
        public ICommand EditProfileCommand { get; }
        public const string RegisterMessage = "REGISTER";
        public const string LoginMessage = "LOGIN";
        public const string SelectTalentsMessage = "SELECT_TALENTS";
        public const string EditUserProfileMessage = "EDIT_PROFILE";

        public event EventHandler ExpandStateChanged;

        public bool IsPresented
        {
            get => GetField<bool>();
            set => SetField(value);
        }

        public bool UserExpanded
        {
            get => GetField<bool>();
            set
            {
                if (value == false || User != null)
                {
                    SetField(value);
                    ExpandStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IUser User
        {
            get => GetField<IUser>();
            set { SetField(value); if (value == null) UserExpanded = false; }
        }

        public MainPageMasterViewModel(SessionHandler sessionHandler) : base(sessionHandler)
        {
            LoginCommand = new Command(() => MessagingCenter.Send(this, LoginMessage));
            LogoutCommand = new Command(() => SessionHandler.Logout());
            RegisterCommand = new Command(() => MessagingCenter.Send(this, RegisterMessage));
            SelectTalentsCommand = new Command(() => MessagingCenter.Send(this, SelectTalentsMessage));
            ProfileExpanderCommand = new Command(() => UserExpanded = !UserExpanded);
            EditProfileCommand = new Command(() => MessagingCenter.Send(this, EditUserProfileMessage, User));
            SessionHandler.UserChanged += (object sender, UserChangedEventArgs e) => User = e.User;
            Login();
        }

        private async void Login()
        {
            IsBusy = true;
            try
            {
                await SessionHandler.OpenbiUserLogin();
            }
            catch (Exception) { }
            finally
            {
                IsBusy = false;
            }
        }
    }
}