using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Models;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MajaMobile.Pages
{
    public partial class TalentsPage : ContentPageBase
    {
        public TalentsPage()
        {
            InitializeComponent();
            BindingContext = ViewModel = new TalentsViewModel();
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class TalentsViewModel : ViewModelBase
    {

        public ObservableCollection<IMajaTalent> Talents { get; } = new ObservableCollection<IMajaTalent>();
        public ObservableCollection<object> SelectedTalents { get; } = new ObservableCollection<object>();
        private bool _loaded;

        public TalentsViewModel()
        {
            LoadTalents();
        }

        private async void LoadTalents()
        {
            try
            {
                IsBusy = true;
                var talents = await SessionHandler.Instance.ExecuteOpenbiCommand((s, t) => s.GetMajaTalents(t));
                var categories = new Dictionary<string, MajaCategory>();
                foreach (var apiTalent in talents)
                {
                    MajaCategory category = null;
                    if (!categories.TryGetValue(apiTalent.Category.Id, out category))
                    {
                        categories[apiTalent.Category.Id] = category = new MajaCategory(apiTalent.Category);
                    }
                    var talent = new MajaTalent(apiTalent, category);
                    Talents.Add(talent);
                    if (SessionHandler.Packages.Contains(talent.Id))
                        SelectedTalents.Add(talent);
                }
                _loaded = true;
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

        public override void SendDisappearing()
        {
            base.SendDisappearing();
            if (_loaded)
            {
                SessionHandler.SetPackages(SelectedTalents.Cast<IMajaTalent>());
            }
        }
    }
}