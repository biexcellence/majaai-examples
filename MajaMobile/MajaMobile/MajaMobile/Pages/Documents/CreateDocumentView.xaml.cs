using BiExcellence.OpenBi.Api;
using BiExcellence.OpenBi.Api.Commands;
using MajaMobile.Commands;
using MajaMobile.Utilities;
using System;
using System.Net.Http;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public class CreateDocumentPage : DocumentPageBase
    {
        public CreateDocumentPage(SessionHandler sessionHandler) : base(new CreateDocumentViewModel(sessionHandler))
        {

        }

        protected override View GetView()
        {
            return new CreateDocumentView();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel is CreateDocumentViewModel vm)
            {
                vm.FileUploaded += FileUploaded;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (ViewModel is CreateDocumentViewModel vm)
            {
                vm.FileUploaded -= FileUploaded;
            }
        }

        private async void FileUploaded(object sender, StringEventArgs e)
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new DocumentDetailPage(ViewModel.SessionHandler, e.Result));
            });
        }

    }

    public partial class CreateDocumentView : ContentView
    {
        public CreateDocumentView()
        {
            InitializeComponent();
        }

    }

    public class CreateDocumentViewModel : DocumentViewModelBase
    {
        public ICommand UploadFileCommand { get; }
        public ICommand TakePictureCommand { get; }
        public event EventHandler<StringEventArgs> FileUploaded;

        public CreateDocumentViewModel(SessionHandler session) : base(session)
        {
            UploadFileCommand = new Command(UploadFile);
            TakePictureCommand = new Command(TakePicture);
        }

        private async void TakePicture()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    Upload(photo);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private async void UploadFile()
        {

            var file = await FilePicker.PickAsync(new PickOptions() { FileTypes = FilePickerFileType.Pdf });
            if (file != null)
            {
                Upload(file);
            }
        }

        private async void Upload(FileResult file)
        {
            try
            {
                using (var stream = await file.OpenReadAsync())
                {
                    using (var client = new HttpClient())
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(new StreamContent(stream), "file1", file.FileName);

                        formData.Headers.TryAddWithoutValidation("session-handle", (await SessionHandler.Session.Ping()).Handle);

                        var response = await client.PostAsync(new Uri(SessionHandler.Session.Configuration.Uri(), "/majaocr"), formData);
                        response.EnsureSuccessStatusCode();
                        var id = await response.Content.ReadAsStringAsync();
                        await SessionHandler.ExecuteOpenbiCommand((s, t) => s.AnalyzeOcrDocument(id));
                        FileUploaded?.Invoke(this, new StringEventArgs(id));
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }
    }

    public class StringEventArgs : EventArgs
    {
        public string Result { get; }
        public StringEventArgs(string s)
        {
            Result = s;
        }
    }
}