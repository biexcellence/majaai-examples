﻿using BiExcellence.OpenBi.Api.Commands.MajaAi;
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
        public TalentsPage(SessionHandler sessionHandler)
        {
            InitializeComponent();
            BindingContext = ViewModel = new TalentsViewModel(sessionHandler);
        }

        private async void TalentList_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            if (Navigation.NavigationStack.Last() == this && e.ItemData is MajaTalent talent)
            {
                await Navigation.PushAsync(new TalentDetailPage(talent, ViewModel.SessionHandler));
            }
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class TalentsViewModel : ViewModelBase
    {

        public ObservableCollection<MajaTalent> Talents { get; } = new ObservableCollection<MajaTalent>();

        public TalentsViewModel(SessionHandler sessionHandler) : base(sessionHandler)
        {
            LoadTalents();
        }

        private async void LoadTalents()
        {
            try
            {
                IsBusy = true;
                var talents = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetMajaTalents(t));
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
                        talent.Selected = true;
                }
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