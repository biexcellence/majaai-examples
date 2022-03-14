using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Utilities;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public partial class CreateEntityPopup : PopupPage
    {
        public event EventHandler<EntityEventArgs> PopupClosed;

        private SessionHandler _sessionHandler;
        private string _baseEntityId;

        private string _text;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        public string BaseEntityName { get; }

        public CreateEntityPopup(SessionHandler sessionHandler, string baseEntityId, string baseEntityName)
        {
            InitializeComponent();
            _sessionHandler = sessionHandler;
            _baseEntityId = baseEntityId;
            BaseEntityName = baseEntityName;
            BindingContext = this;
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

        private async void Accept_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                await DisplayAlert("Kein Name", "Bitte geben Sie einen Namen ein", "Ok");
                return;
            }
            try
            {
                var entity = new Entity();
                entity.Name = Text;
                if (_baseEntityId == "tag")
                {
                    entity.CustomAttributes["organisation"] = _sessionHandler.Organisation?.Id;
                }
                await _sessionHandler.ExecuteOpenbiCommand((s, t) => s.CreateEntity(_baseEntityId, entity));
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    PopupClosed?.Invoke(this, new EntityEventArgs(entity));
                    await Navigation.PopPopupAsync();
                });
            }
            catch (Exception ex)
            {
                await Device.InvokeOnMainThreadAsync(() => DocumentPageBase.DisplayException(ex, this));
            }
        }
    }
}