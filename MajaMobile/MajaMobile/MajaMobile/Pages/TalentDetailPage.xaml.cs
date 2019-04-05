using BiExcellence.OpenBi.Api.Commands.Organisations;
using MajaMobile.Models;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class TalentDetailPage : ContentPageBase
    {
        public TalentDetailPage(MajaTalent talent, SessionHandler sessionHandler)
        {
            InitializeComponent();
            BindingContext = ViewModel = new TalentDetailViewModel(talent, sessionHandler);
            TalentActiveLabel.SizeChanged += TalentActiveLabel_SizeChanged;
        }

        private void TalentActiveLabel_SizeChanged(object sender, EventArgs e)
        {
            //different heights of labels on different devices require dynamic vertical margins.
            //small labelHeights require high vertical margin
            if (TalentActiveLabel.Height > 0)
            {
                TalentActiveLabel.SizeChanged -= TalentActiveLabel_SizeChanged;
                var labelHeight = TalentActiveLabel.Height;
                //heights on all tested devices were 20-28
                var margin = 35 - labelHeight;
                CategoryLabel.Margin = TalentLabel.Margin = OrganisationLabel.Margin = DescriptionLabel.Margin = new Thickness(20, margin);
                TalentActiveLabel.Margin = new Thickness(20, margin, Device.RuntimePlatform == Device.iOS ? margin * 0.5 : margin, margin);
            }
        }

        private async void Showcase_Clicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Last() == this)
                await Navigation.PushAsync(new TalentShowcasePage(((TalentDetailViewModel)ViewModel).Talent));
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class TalentDetailViewModel : ViewModelBase
    {
        public ICommand TalentSelectionCommand { get; }
        public MajaTalent Talent { get; }
        public IOrganisation Organisation
        {
            get => GetField<IOrganisation>();
            private set { SetField(value); }
        }

        public bool TalentSelected
        {
            get => Talent.Selected;
            set
            {
                Talent.Selected = value;
                OnPropertyChanged();
                SessionHandler.SaveTalentSelection(Talent);
            }
        }

        public TalentDetailViewModel(MajaTalent talent, SessionHandler sessionHandler) : base(sessionHandler)
        {
            Talent = talent;
            LoadOrganisation(talent.OrganisationId);
            TalentSelectionCommand = new Command(() => TalentSelected = !TalentSelected);
        }

        private async void LoadOrganisation(string organisationId)
        {
            IsBusy = true;
            try
            {
                Organisation = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetOrganisationById(organisationId));
            }
            catch (Exception e)
            {
                DisplayException(e);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}