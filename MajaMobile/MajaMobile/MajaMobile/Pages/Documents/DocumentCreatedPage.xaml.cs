using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public partial class DocumentCreatedPage : ContentPage
    {
        private bool _fromWizard;

        public DocumentCreatedPage(bool fromWizard)
        {
            InitializeComponent();
            _fromWizard = fromWizard;
        }

        private async void AnimationView_OnFinishedAnimation(object sender, System.EventArgs e)
        {
            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count-2]);
            if (_fromWizard)
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count-2]);
            await Navigation.PopAsync();
        }
    }
}