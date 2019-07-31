using BiExcellence.OpenBi.Api.Commands.MajaAi;
using BiExcellence.OpenBi.Api.Commands.Organisations;
using GalaSoft.MvvmLight.Messaging;
using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        SettingsPageViewmodel viewmodel;
        MajaConversation conversation;

        public SettingsPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            conversation = (MajaConversation) e.Parameter;
            
            DataContext = viewmodel = new SettingsPageViewmodel();
            SetupVolume();

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AppSettingHandler.SetAppSetting("speakVolume", SpeakSlider.Value / 100);
            AppSettingHandler.SetAppSetting("alarmVolume", AlarmSlider.Value / 100);

            base.OnNavigatedFrom(e);
        }

        private void SetupVolume()
        {
            var speakerVolume = AppSettingHandler.GetAppSetting("speakVolume");
            if (speakerVolume != null)
            {
                SpeakSlider.Value = Convert.ToInt32((double)speakerVolume *100);
            }
            else
            {
                SpeakSlider.Value = 50;
            }

            var alarmVolume = AppSettingHandler.GetAppSetting("alarmVolume");
            if (speakerVolume != null)
            {
                AlarmSlider.Value = Convert.ToInt32((double) alarmVolume*100);
            }
            else
            {
                AlarmSlider.Value = 50;
            }
        }

        private async void ShowCaseTapped(object sender, TappedRoutedEventArgs e)
        {
            Border b = sender as Border;
            TextBlock tb = b.Child as TextBlock;
            this.Frame.GoBack();
            await conversation.QueryMajaForAnswers(tb.Text);

        }

        private async void LoginButton_Clicked(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async System.Threading.Tasks.Task Login()
        {
            string username = UserBox.Text;
            string passWord = PassBox.Password;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username))
            {
                StatusText.Text = "";
                try
                {
                    SessionHandler testSession = new SessionHandler();
                    await testSession.OpenbiUserLogin(username, passWord);
                    LoginOverlay.Hide();
                    AppSettingHandler.StoreCredentials(username, passWord);
                    viewmodel.IsMajaLoggedIn = true;
                    viewmodel.SetupLists();
                }
                catch (Exception ex)
                {
                    StatusText.Text = ex.Message;
                }

            }
            else
            {
                StatusText.Text = "Bitte Benutzername und Password angeben";
            }
        }

        private void LogoutButton_Clicked(object sender, RoutedEventArgs e)
        {
            AppSettingHandler.Logout();
            viewmodel.SetupLists();
            viewmodel.IsMajaLoggedIn = false;
        }

        private async void PassBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
               await Login();
            }
        }

        private void RemoveFromListButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Talent talent)
            {
                Utils.RemovePackage(talent.Id);
                button.Visibility = Visibility.Collapsed;

                StackPanel sp = button.Parent as StackPanel;

                ProgressRing pr = sp.Children[1] as ProgressRing;
                pr.Visibility = Visibility.Visible;
                viewmodel.SetupLists();
            }
        }

        private void AddTalentButton_Clicked(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<string>("openTalentPickerPage");
        }

    }


}
namespace MajaUWP.ViewModels
{

    public class SettingsPageViewmodel : ViewModelBase
    {
        private bool isMajaLoggedIn;

        public ObservableCollection<Talent> AllTalents { get; set; }
        public ObservableCollection<Talent> InstalledTalents { get; set; }

        public Dictionary<string, string> organisationNames { get; set; }
        public ICommand MsLogoutCommand { get; set; }
        public ICommand TalentResetCommand { get; set; }
        public bool IsMajaLoggedIn { get => isMajaLoggedIn; set { isMajaLoggedIn = value; OnPropertyChanged(nameof(IsMajaLoggedIn)); SetupLogin(); } }
        public string MajaUserName { get; set; }
        public bool IsMicrosoftLoggedIn { get; set; }

        public SettingsPageViewmodel()
        {
            SetupLogin();
            SetupLists();
            IsMicrosoftLoggedIn = !string.IsNullOrEmpty(Utils.microSoftToken);

            MsLogoutCommand = new Command(() =>
            {
                Utils.microSoftToken = "";
                IsMicrosoftLoggedIn = false;
                OnPropertyChanged(nameof(IsMicrosoftLoggedIn));
            });
            TalentResetCommand = new Command(() =>
            {
                Utils.MajaPackages = Utils.DefaultPackages;
                SetupLists();
            });

        }

        public async void SetupLists()
        {
            

            IsMajaLoggedIn = Utils.IsMajaLoggedIn;
            OnPropertyChanged(nameof(IsMajaLoggedIn));

            SessionHandler sessionHandler = new SessionHandler();
            await sessionHandler.LoginWithSavedCredential();
            IList<IMajaTalent> talents = await sessionHandler.ExecuteOpenbiCommand((s, t) => s.GetMajaTalents());
            AllTalents = new ObservableCollection<Talent>();

            foreach (var ITalent in talents)
            {
                AllTalents.Add(new Talent(ITalent, sessionHandler));
            }

            InstalledTalents = new ObservableCollection<Talent>();
            foreach (var talent in AllTalents)
            {
                if (Utils.MajaPackages.Contains(talent.Id))
                {
                    InstalledTalents.Add(talent);
                }
                if (talent.ImagePath == null)
                {

                }

            }
            OnPropertyChanged(nameof(AllTalents));
            OnPropertyChanged(nameof(InstalledTalents));
        }

        private async System.Threading.Tasks.Task SetupLogin()
        {
            if (Utils.IsMajaLoggedIn)
            {
                var cred = await AppSettingHandler.GetCredentials();
                MajaUserName = $"Eingeloggt als {cred.UserName}";
            }
            else MajaUserName = "";
            OnPropertyChanged(nameof(MajaUserName));
        }
    }

    public class Talent : IMajaTalent
    {
        

        public string Id {get;}

        public string Name {get;}

        public string Description {get;}

        public string ImagePath {get;}

        public bool IsPublic {get;}

        public string OrganisationId {get;}

        public IMajaTalentCategory Category {get;}

        public IList<IMajaGrammar> ShowcaseGrammars {get;}
        public string Author { get; protected set; }


        public Talent( IMajaTalent talent, SessionHandler sh)
        {
            Id = talent.Id;
            Name = talent.Name;
            ImagePath = talent.ImagePath;
            if (!string.IsNullOrEmpty(ImagePath) && ImagePath.StartsWith("//")) ImagePath = "https:" + ImagePath;
            IsPublic = talent.IsPublic;
            Description = talent.Description;
            OrganisationId = talent.OrganisationId;
            ShowcaseGrammars = talent.ShowcaseGrammars.ToList();
            Category = talent.Category;
            SetAuthor(sh);
        }

        public async void SetAuthor(SessionHandler sessionHandler)
        {
            if (Utils.IsMajaLoggedIn)
            {
                IOrganisation org = await sessionHandler.ExecuteOpenbiCommand((s, t) => s.GetOrganisationById(OrganisationId));
                Author = org.Name;
            }
                
        }






    }


}

namespace MajaUWP.Converters
{
    public class MajaImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if ( value is string v && !string.IsNullOrEmpty(v))
            {
               

                return value;
            }

            else if(parameter is string s && s == "highRes")
                return @"ms-appx:///Assets/maja.png";
            return @"ms-appx:///Assets/maja_thumbnail.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

       
    }

    



    public class DefaultPackageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string packageId && !Utils.DefaultPackages.Contains(packageId))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }





}
