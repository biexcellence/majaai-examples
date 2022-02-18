using MajaMobile.Pages;
using MajaMobile.Utilities;
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

            AppDatabase.UpdateDatabase(); 
            MainPage = new MainPageMasterDetail();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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
