using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Utilities;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public partial class SelectTagPopup : PopupPage
    {
        public event EventHandler<EntityEventArgs> PopupClosed;

        private SessionHandler _sessionHandler;

        private IEntity _selectedTag;
        public IEntity SelectedTag
        {
            get => _selectedTag;
            set { _selectedTag = value; OnPropertyChanged(); }
        }

        public ObservableCollection<IEntity> Tags { get; } = new ObservableCollection<IEntity>();

        public SelectTagPopup(SessionHandler sessionHandler, IEnumerable<string> selectedTags)
        {
            InitializeComponent();
            BindingContext = this;
            _sessionHandler = sessionHandler;
            LoadTags(selectedTags);
        }

        private async void LoadTags(IEnumerable<string> selectedTags)
        {
            try
            {
                var tags = await _sessionHandler.ExecuteOpenbiCommand((s, t) =>
                {
                    var request = s.GetEntities("tag").AddField("Name").AddField("ID").AddFilter("organisation", new Filter(_sessionHandler.Organisation?.Id, FilterOperator.Equal)).SortAscending("Name");
                    foreach (var tag in selectedTags)
                    {
                        request.AddFilter("ID", new Filter(tag, FilterOperator.Equal, FilterSign.Exclude));
                    }
                    return request.Send();
                });
                foreach (var tag in tags)
                {
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                await Device.InvokeOnMainThreadAsync(() => DocumentPageBase.DisplayException(ex, this));
            }
        }

        protected override bool OnBackButtonPressed()
        {
            PopupClosed?.Invoke(this, new EntityEventArgs(null));
            return false;
        }

        private async void Close_Clicked(object sender, EventArgs e)
        {
            PopupClosed?.Invoke(this, new EntityEventArgs(null));
            await Navigation.PopPopupAsync();
        }

        private async void NewTag_Clicked(object sender, EventArgs e)
        {
            var popup = new CreateEntityPopup(_sessionHandler, "tag", "Zuordnung");
            popup.PopupClosed += Popup_PopupClosed;
            await Navigation.PushPopupAsync(popup);
        }

        private void Popup_PopupClosed(object sender, EntityEventArgs e)
        {
            if (sender is CreateEntityPopup popup)
            {
                popup.PopupClosed -= Popup_PopupClosed;
            }
            if (e.Entity != null)
            {
                Tags.Add(e.Entity);
                SelectedTag = e.Entity;
            }
        }

        private async void Accept_Clicked(object sender, EventArgs e)
        {
            PopupClosed?.Invoke(this, new EntityEventArgs(SelectedTag));
            await Navigation.PopPopupAsync();
        }
    }

    public class EntityEventArgs : EventArgs
    {
        public IEntity Entity { get; }
        public EntityEventArgs(IEntity entity)
        {
            Entity = entity;
        }
    }
}