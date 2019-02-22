using MajaMobile.Pages;
using MajaMobile.Utilities;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace MajaMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPageMasterDetail();
        }

        private bool _started;
        protected override async void OnStart()
        {
            // Handle when your app starts
            if (_started)
                return;
            _started = true;
            try
            {
                await SessionHandler.Instance.OpenbiUserLogin();
            }
            catch (Exception) { }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
