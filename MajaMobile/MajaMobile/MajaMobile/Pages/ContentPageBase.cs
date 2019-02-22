using BiExcellence.OpenBi.Api;
using MajaMobile.ViewModels;
using System;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public class ContentPageBase : ContentPage
    {

        public ViewModelBase ViewModel { get; protected set; }

        public ContentPageBase()
        {
            var style = (Style)Application.Current.Resources["ContentPageStyle"];
            Style = style;
        }

        private string _title;
        protected bool PageIsActive;

        protected override void OnAppearing()
        {
            if (!string.IsNullOrEmpty(_title))
            {
                Title = _title;
            }
            base.OnAppearing();
            if (ViewModel != null)
            {
                MessagingCenter.Subscribe<ViewModelBase, Exception>(this, ViewModelBase.OpenbirequestErrorMessage, RequestOnError);
                MessagingCenter.Subscribe(this, ViewModelBase.GoBackMessage, async (ViewModelBase vm) => await Navigation.PopAsync());
                ViewModel.SendAppearing();
            }
            PageIsActive = true;
        }

        protected override void OnDisappearing()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                _title = Title;
                Title = null;
            }
            base.OnDisappearing();
            if (ViewModel != null)
            {
                MessagingCenter.Unsubscribe<ViewModelBase, Exception>(this, ViewModelBase.OpenbirequestErrorMessage);
                MessagingCenter.Unsubscribe<ViewModelBase>(this, ViewModelBase.GoBackMessage);
                ViewModel.SendDisappearing();
            }
            PageIsActive = false;
        }

        public async void RequestOnError(ViewModelBase viewmodel, Exception ex)
        {
            if (viewmodel == ViewModel)
            {
                var message = ex.Message;
                if (ex is OpenBiServerErrorException openBiServerError)
                {
                    if (openBiServerError.Response.Code == OpenBiResponseCodes.LoginFailed)
                    {
                        message = "Benutzername oder Passwort falsch";
                    }
                }
                await DisplayAlert("Fehler", message, "OK");
            }
        }
    }
}