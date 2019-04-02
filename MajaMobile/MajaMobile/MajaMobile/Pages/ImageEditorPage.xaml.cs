using MajaMobile.ViewModels;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class ImageEditorPage : CancelBackContentPage
    {
        public const string ImageSavedMessage = "IMAGE_SAVED";
        private bool _saved;

        public ImageEditorPage(byte[] img)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ImageEditorViewModel(img);

            //https://help.syncfusion.com/xamarin/sfimageeditor/toolbarcustomization
            var itemNames = ImageEditor.ToolbarSettings.ToolbarItems.Select(i => i.Name.ToLower()).Except(new[] { "transform" }).ToList();
            itemNames.Add("rotate");
            if (Device.RuntimePlatform == Device.iOS)
                itemNames.Add("flip"); //looks like flip doesn't work on iOS after save
            ImageEditor.SetToolbarItemVisibility(string.Join(",", itemNames), false);
        }

        private async void SfImageEditor_ImageSaving(object sender, Syncfusion.SfImageEditor.XForms.ImageSavingEventArgs args)
        {
            args.Cancel = true;
            using (var ms = new MemoryStream())
            {
                args.Stream.CopyTo(ms);
                MessagingCenter.Send(this, ImageSavedMessage, ms.ToArray());
                _saved = true;
                await Navigation.PopAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (!_saved)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert("Bild bearbeiten", "Änderungen verwerfen?", "VERWERFEN", "ABBRECHEN"))
                    {
                        _saved = true;
                        await Navigation.PopAsync();
                    }
                });
                return true;
            }
            return false;
        }

        public override bool OnBackPressed()
        {
            return OnBackButtonPressed();
        }

        private void Save_Clicked(object sender, System.EventArgs e)
        {
            ImageEditor.Crop();
            ImageEditor.Save();
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class ImageEditorViewModel : ViewModelBase
    {
        public byte[] Image { get; }
        public ImageEditorViewModel(byte[] img)
        {
            Image = img;
        }
    }
}