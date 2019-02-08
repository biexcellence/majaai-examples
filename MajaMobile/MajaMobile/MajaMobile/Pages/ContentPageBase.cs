using MajaMobile.ViewModels;
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
                MessagingCenter.Unsubscribe<ViewModelBase>(this, ViewModelBase.GoBackMessage);
                ViewModel.SendDisappearing();
            }
            PageIsActive = false;
        }
    }
}