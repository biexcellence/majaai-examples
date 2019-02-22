using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class LoginPage : ContentPageBase
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = ViewModel = new LoginPageViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe(this, LoginPageViewModel.LoginMessage, async (LoginPageViewModel viewModel) => await Navigation.PopAsync());
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<LoginPageViewModel>(this, LoginPageViewModel.LoginMessage);
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        public ICommand SignInCommand { get; }
        public const string LoginMessage = "USER_LOGGED_IN";

        public string UsernameInput
        {
            get => GetField<string>();
            set
            {
                SetField(value);
                OnPropertyChanged(nameof(LoginValid));
            }
        }

        public string PasswordInput
        {
            get => GetField<string>();
            set
            {
                SetField(value);
                OnPropertyChanged(nameof(LoginValid));
            }
        }

        public virtual bool LoginValid
        {
            get
            {
                return !string.IsNullOrEmpty(UsernameInput) && !string.IsNullOrEmpty(PasswordInput);
            }
        }

        public LoginPageViewModel()
        {
            SignInCommand = new Command(SignIn);
        }

        protected virtual async void SignIn()
        {
            if (LoginValid && IsIdle)
            {
                IsBusy = true;
                try
                {
                    await SessionHandler.Instance.OpenbiUserLogin(UsernameInput, PasswordInput);
                    MessagingCenter.Send(this, LoginMessage);
                }
                catch (Exception ex)
                {
                    DisplayException(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}