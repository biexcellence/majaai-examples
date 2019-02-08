using Android.App;
using Android.Content.PM;
using Android.OS;
using Syncfusion.Licensing;
using System;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace MajaMobile.Droid
{
    [Activity(Label = "majaAI", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            SyncfusionLicenseProvider.RegisterLicense("TODO: YOURLICENSEKEY");

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;

            App.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
            AppDomain.CurrentDomain.UnhandledException += onUnhandledException;
        }
        private void onUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                var message = ex.Message;
                var stack = ex.StackTrace;
            }
        }
    }
}