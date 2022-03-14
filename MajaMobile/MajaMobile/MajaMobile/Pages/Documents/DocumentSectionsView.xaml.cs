using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Utilities;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public partial class DocumentSectionsView : ContentView, IDocumentPageContent
    {
        public DocumentSectionsView()
        {
            InitializeComponent();
        }

        public void SendAppearing()
        {
            if (BindingContext is DocumentSectionsViewModel vm)
            {
                vm.ProgressChanged +=Vm_ProgressChanged;
                vm.Finished +=Vm_Finished;
            }
        }

        public void SendDisappearing()
        {
            if (BindingContext is DocumentSectionsViewModel vm)
            {
                vm.ProgressChanged -=Vm_ProgressChanged;
                vm.Finished -=Vm_Finished;
            }
        }

        private async void Vm_Finished(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DocumentCreatedPage(true));
        }

        private async void Vm_ProgressChanged(object sender, EventArgs e)
        {
            if (sender is DocumentSectionsViewModel vm)
            {
                await SectionsProgress.ProgressTo(vm.Progress, 1000, Easing.CubicOut);
            }
        }

        private void BaseEntities_SelectionChanged(object sender, Controls.CustomSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is IBaseEntity baseEntity && BindingContext is DocumentSectionsViewModel vm)
            {
                vm.ChangeBaseEntity(baseEntity);
            }
        }

        private async void Entities_SelectionChanged(object sender, Controls.CustomSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is DocumentSectionsViewModel.NewEntityDummy && BindingContext is DocumentSectionsViewModel vm)
            {
                vm.SelectedEntity = null;
                var popup = new CreateEntityPopup(vm.SessionHandler, vm.SelectedBaseEntity.Id, vm.SelectedBaseEntity.Name);
                popup.PopupClosed += Popup_PopupClosed;
                await Navigation.PushPopupAsync(popup);
            }
        }

        private void Popup_PopupClosed(object sender, EntityEventArgs e)
        {
            if (sender is CreateEntityPopup popup)
            {
                popup.PopupClosed -= Popup_PopupClosed;
            }
            if (e.Entity != null && BindingContext is DocumentSectionsViewModel vm)
            {
                vm.Entities.Add(e.Entity);
                vm.SelectedEntity = e.Entity;
            }
        }
    }

    public class DocumentSectionsPage : DocumentPageBase
    {
        public ICommand CloseCommand { get; }
        public ICommand DeleteCommand { get; }

        public DocumentSectionsPage(SessionHandler sessionHandler, IEnumerable<OcrDocumentSection> sections) : base(new DocumentSectionsViewModel(sessionHandler, sections))
        {
            CloseCommand = new Command(ClosePage);
            DeleteCommand = new Command(DeleteSection);
            OnPropertyChanged(nameof(CloseCommand));
            OnPropertyChanged(nameof(DeleteCommand));
        }

        private async void DeleteSection(object obj)
        {
            if (ViewModel is DocumentSectionsViewModel vm && vm.CurrentSection != null)
            {
                if (await DisplayAlert("Löschen", $"Wollen Sie den erkannten Textabschnitt {vm.CurrentSection.Name} entfernen?", "Ja", "Nein"))
                {
                    vm.DeleteSection();
                }
            }
        }

        private async void ClosePage(object obj)
        {
            if (await DisplayAlert("Abbrechen", "Wollen Sie das Bearbeiten abbrechen?", "Ja", "Nein"))
            {
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count-2]);
                await Navigation.PopAsync();
            }
        }

        protected override View GetView()
        {
            return new DocumentSectionsView();
        }
    }

    public class DocumentSectionsViewModel : DocumentViewModelBase
    {
        private int _currentIndex;
        private List<OcrDocumentSection> _sections;

        public event EventHandler ProgressChanged;
        public event EventHandler Finished;
        public ICommand LastSectionCommand { get; }
        public ICommand SaveSectionCommand { get; }
        public ICommand DeleteSectionCommand { get; }

        public bool CanGoBack => _currentIndex > 0;
        public OcrDocumentSection CurrentSection => _sections[_currentIndex];
        public double Progress => (double)_currentIndex / _sections.Count;

        public ObservableCollection<IBaseEntity> BaseEntities { get; } = new ObservableCollection<IBaseEntity>();
        public ObservableCollection<IEntity> Entities { get; } = new ObservableCollection<IEntity>();

        public IBaseEntity SelectedBaseEntity
        {
            get => GetField<IBaseEntity>();
            set => SetField(value);
        }

        public IEntity SelectedEntity
        {
            get => GetField<IEntity>();
            set => SetField(value);
        }

        public DocumentSectionsViewModel(SessionHandler sessionHandler, IEnumerable<OcrDocumentSection> sections) : base(sessionHandler)
        {
            _sections = new List<OcrDocumentSection>(sections);
            LastSectionCommand = new Command(GoBackSection);
            SaveSectionCommand = new Command(SaveSection);
            LoadCurrentSection();
        }

        private async void SaveSection()
        {
            try
            {
                if (SelectedBaseEntity == null)
                {
                    throw new Exception("Bitte wählen Sie eine Basisentität aus");
                }
                if (SelectedEntity == null)
                {
                    throw new Exception("Bitte wählen Sie eine Entität aus");
                }
                using (Busy())
                {
                    CurrentSection.BaseEntity = SelectedBaseEntity?.Id;
                    CurrentSection.EntityId = SelectedEntity?.Id;
                    await SessionHandler.ExecuteOpenbiCommand((s, t) => s.CreateEntity("ocr_document_section", CurrentSection));
                    _currentIndex++;
                    LoadCurrentSection();
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void GoBackSection()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                LoadCurrentSection();
            }
        }

        public async void DeleteSection()
        {
            try
            {
                using (Busy())
                {
                    await SessionHandler.ExecuteOpenbiCommand((s, t) => s.DeleteEntity("ocr_document_section", CurrentSection.Id));
                    _sections.RemoveAt(_currentIndex);
                    LoadCurrentSection();
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private async void LoadCurrentSection()
        {
            if (_currentIndex <_sections.Count)
            {

                try
                {
                    using (Busy())
                    {
                        if (BaseEntities.Count ==0)
                        {
                            foreach (var baseEntity in await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetBaseEntities().AddField("ID").AddField("Name").SortAscending("Name").Send()))
                            {
                                BaseEntities.Add(baseEntity);
                            }
                        }
                        Entities.Clear();
                        if (!string.IsNullOrEmpty(CurrentSection.BaseEntity))
                        {
                            var entities = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntities(CurrentSection.BaseEntity).AddField("ID").AddField("Name").SortAscending("Name").Send());
                            Entities.Add(new NewEntityDummy());
                            foreach (var entity in entities)
                            {
                                Entities.Add(entity);
                                if (entity.Id == CurrentSection.EntityId)
                                {
                                    SelectedEntity = entity;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayException(ex);
                }

                SelectedBaseEntity = BaseEntities.FirstOrDefault(b => b.Id == CurrentSection.BaseEntity);

                OnPropertyChanged(nameof(CurrentSection));
                OnPropertyChanged(nameof(CanGoBack));
                ProgressChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Finished?.Invoke(this, EventArgs.Empty);
            }
        }

        public async void ChangeBaseEntity(IBaseEntity baseEntity)
        {
            try
            {
                using (Busy())
                {
                    SelectedEntity = null;
                    Entities.Clear();
                    var entities = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntities(baseEntity).AddField("ID").AddField("Name").SortAscending("Name").Send());
                    Entities.Add(new NewEntityDummy());
                    foreach (var entity in entities)
                    {
                        Entities.Add(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        public class NewEntityDummy : Entity
        {
            public NewEntityDummy()
            {
                Name = "Neue Entität anlegen";
            }
        }
    }
}