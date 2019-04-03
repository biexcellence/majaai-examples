using BiExcellence.OpenBi.Api.Commands.Organisations;
using MajaMobile.Models;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System;
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
            OrganisationLabel.SizeChanged += OrganisationLabel_SizeChanged;
        }

        private void OrganisationLabel_SizeChanged(object sender, EventArgs e)
        {
            //different heights of labels on different devices require dynamic vertical margins.
            //small labelHeights require high vertical margin
            if (CategoryLabel.Height > 0)
            {
                OrganisationLabel.SizeChanged -= OrganisationLabel_SizeChanged;
                var labelHeight = OrganisationLabel.Height;
                //heights on all tested devices were 20-28
                CategoryLabel.Margin = TalentLabel.Margin = OrganisationLabel.Margin = DescriptionLabel.Margin = new Thickness(20, 35 - labelHeight);
            }
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

        public TalentDetailViewModel(MajaTalent talent, SessionHandler sessionHandler) : base(sessionHandler)
        {
            Talent = talent;
            LoadOrganisation(talent.OrganisationId);
            TalentSelectionCommand = new Command(() =>
            {
                Talent.Selected = !Talent.Selected;
                SessionHandler.SaveTalentSelection(Talent);
            });
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