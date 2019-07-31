using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MicrosoftLoginPage : Page
    {

        string[] scopes = Utils.MicrosoftPermissionScopes;
        public IPublicClientApplication PublicClientApp { get; }
        string outputText { get; set; }
        private MajaConversation _conversation;
        MicrosoftLoginPageViewmodel _viewmodel;
        bool clicked = false;
        IEnumerable<IAccount> accounts;



        public MicrosoftLoginPage()
        {
            this.InitializeComponent();

            PublicClientApp = PublicClientApplicationBuilder.Create(Utils.MicrosoftClientId)
              .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
              .WithLogging((level, message, containsPii) =>
              { outputText = "Something went wrong"; }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
              .WithUseCorporateNetwork(true)
              .Build();
            

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is MajaConversation conversation)
            {
                _conversation = conversation;
            }
            
            accounts = await PublicClientApp.GetAccountsAsync();
            
            DataContext = _viewmodel = new MicrosoftLoginPageViewmodel(accounts);
            if (accounts.FirstOrDefault() != null)
            {
                _viewmodel.setAccountLoggedIn( "Mit " + accounts.FirstOrDefault().Username + " anmelden \r\n oder" );
            }

        }


        public async void Login(bool newLogin = false)
        {
            AuthenticationResult authResult = null;
            
            if(newLogin) await PublicClientApp.RemoveAsync(accounts.FirstOrDefault());

            IAccount firstAccount =  accounts.FirstOrDefault();
            

           
                
                try
                {
                    authResult = await PublicClientApp.AcquireTokenSilent(scopes, firstAccount)
                                                      .ExecuteAsync();
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                    try
                    {
                        authResult = await PublicClientApp.AcquireTokenInteractive(scopes)
                                                          .ExecuteAsync();
                    }
                    catch (MsalException msalex)
                    {
                        _viewmodel.setOutputText($"Error Acquiring Token:{Environment.NewLine}{msalex}");
                    }
                }
                catch (Exception ex)
                {
                    _viewmodel.setOutputText($"Error Acquiring Token Silently:{Environment.NewLine}{ex}");
                    return;
                }

                if (authResult != null)
                {
                    try
                    {
                        this.Frame.GoBack();
                    }
                    catch (Exception)
                    {

                    }
                    
                    Utils.microSoftToken = authResult.AccessToken;
                Utils.SetTokenExpirationTimer(authResult.ExpiresOn);

                    try
                    {
                        await _conversation.QueryMajaForAnswers("Login");

                    }
                    catch (Exception)
                    {


                    }
                }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!clicked)
            {
                Login();
                clicked = true;
            }
        }

        private void ButtonClickNewAccount(object sender, RoutedEventArgs e)
        {
            if (!clicked)
            {
                Login(true);
                clicked = true;
            }
            
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class MicrosoftLoginPageViewmodel : ViewModelBase
    {
        string outputText { get; set; }
        public string AccountLoggedIn { get; set; }

        public IEnumerable<IAccount> accounts { get; set; }
        public MicrosoftLoginPageViewmodel(IEnumerable<IAccount> acc)
        {
            accounts = acc;   
        }

        public void setAccountLoggedIn(string toSet) {
            AccountLoggedIn = toSet;
            OnPropertyChanged(nameof(AccountLoggedIn));
        }

        public void setOutputText(string toSet){
            outputText = toSet;
            OnPropertyChanged(nameof(outputText));
        }
        






    }
}
