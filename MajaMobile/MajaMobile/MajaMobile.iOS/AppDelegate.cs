using AVFoundation;
using Foundation;
using ObjCRuntime;
using Syncfusion.Licensing;
using Syncfusion.ListView.XForms.iOS;
using Syncfusion.SfAutoComplete.XForms.iOS;
using Syncfusion.SfImageEditor.XForms.iOS;
using UIKit;
using Xamarin.Forms;

namespace MajaMobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();

            // SYNCFUSION
            SyncfusionLicenseProvider.RegisterLicense(""); //TODO: Your Syncfusion License
            SfListViewRenderer.Init();
            SfAutoCompleteRenderer.Init();
            SfImageEditorRenderer.Init();
            Sharpnado.Shades.iOS.iOSShadowsRenderer.Initialize();
            Syncfusion.XForms.iOS.Buttons.SfChipRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfChipGroupRenderer.Init();
            Syncfusion.XForms.iOS.Border.SfBorderRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfButtonRenderer.Init(); 
            Syncfusion.XForms.iOS.ComboBox.SfComboBoxRenderer.Init();
            FormsMaterial.Init();

            LoadApplication(new App());

            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);

            return base.FinishedLaunching(app, options);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, [Transient] UIWindow forWindow)
        {
            if (Device.Idiom == TargetIdiom.Tablet)
                return UIInterfaceOrientationMask.AllButUpsideDown;
            return UIInterfaceOrientationMask.Portrait;
        }
    }
}