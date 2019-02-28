using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using Syncfusion.DataSource.Extensions;
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

        public ObservableCollection<MajaTalent> Talents { get; } = new ObservableCollection<MajaTalent>();
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
                SessionHandler.SetPackages(SelectedTalents.Select(t => ((MajaTalent)t).Id));
            }
        }

        public class MajaTalent
        {
            public string Id { get; }
            public string Name { get; }
            public string Image { get; }
            public MajaCategory Category { get; }

            public MajaTalent(IMajaTalent talent, MajaCategory category)
            {
                Id = talent.Id;
                Name = talent.Name;
                Image = talent.ImagePath;
                if (string.IsNullOrEmpty(Image))
                    Image = "maja.png";
                Category = category;
            }
        }

        public class MajaCategory : IComparable<MajaCategory>, IComparable
        {
            public string Id { get; }
            public string Name { get; }

            public MajaCategory(IMajaTalentCategory category)
            {
                Id = category.Id;
                Name = category.Name;
            }

            public int CompareTo(MajaCategory other)
            {
                return Name.CompareTo(other.Name);
            }

            //Necessary for SfListView grouping
            public int CompareTo(object obj)
            {
                if (obj is MajaCategory cat)
                {
                    return CompareTo(cat);
                }
                return -1;
            }
        }
    }
}