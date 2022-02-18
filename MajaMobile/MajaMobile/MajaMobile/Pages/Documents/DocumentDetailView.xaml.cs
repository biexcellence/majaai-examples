using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Utilities;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public partial class DocumentDetailView : ContentView
    {
        public DocumentDetailView(DocumentDetailViewModel vm)
        {
            InitializeComponent();
        }

        private async void Close_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void AddTag_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is DocumentDetailViewModel vm)
            {
                var popup = new SelectTagPopup(vm.SessionHandler, vm.Tags.Select(t => t.Id));
                popup.PopupClosed += Popup_PopupClosed;
                await Navigation.PushPopupAsync(popup);
            }
        }

        private void Popup_PopupClosed(object sender, EntityEventArgs e)
        {
            if (sender is SelectTagPopup popup)
            {
                popup.PopupClosed -= Popup_PopupClosed;
            }
            if (e.Entity != null && BindingContext is DocumentDetailViewModel vm)
            {
                vm.Tags.Insert(vm.Tags.Count - 1, e.Entity);
            }
        }
    }

    public class DocumentDetailPage : DocumentPageBase
    {

        public DocumentDetailPage(SessionHandler sessionHandler, OcrDocument document) : base(new DocumentDetailViewModel(sessionHandler, document))
        {

        }

        public DocumentDetailPage(SessionHandler sessionHandler, string documentId) : base(new DocumentDetailViewModel(sessionHandler, documentId))
        {

        }

        protected override View GetView()
        {
            return new DocumentDetailView(ViewModel as DocumentDetailViewModel);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel is DocumentDetailViewModel vm)
            {
                vm.Accepted += Viewmodel_Accepted;
                vm.ShowSections += Viewmodel_ShowSections;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (ViewModel is DocumentDetailViewModel vm)
            {
                vm.Accepted -= Viewmodel_Accepted;
                vm.ShowSections -= Viewmodel_ShowSections;
            }
        }

        private async void Viewmodel_ShowSections(object sender, EventArgs e)
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new DocumentSectionsPage(ViewModel.SessionHandler, ((DocumentDetailViewModel)ViewModel).DocumentSections));
            });
        }

        private async void Viewmodel_Accepted(object sender, EventArgs e)
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopAsync();
            });
        }
    }

    public class DocumentDetailViewModel : DocumentViewModelBase
    {

        private OcrDocument _document;
        public OcrDocument Document => _document;
        public ICommand CheckSectionsCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public event EventHandler Accepted;
        public event EventHandler ShowSections;
        public ObservableCollection<OcrDocumentSection> DocumentSections { get; } = new ObservableCollection<OcrDocumentSection>();
        public bool CanShowSections => DocumentSections.Count>0;

        private List<IEntity> _documentTypes;
        public List<IEntity> DocumentTypes
        {
            get => _documentTypes;
            set { _documentTypes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<IEntity> Tags { get; } = new ObservableCollection<IEntity>();

        public DocumentDetailViewModel(SessionHandler sessionHandler, OcrDocument document) : base(sessionHandler)
        {
            _document = document;
            AcceptCommand = new Command(Accept);
            CheckSectionsCommand = new Command(CheckSections);
            RemoveTagCommand = new Command((object o) =>
            {
                if (o is IEntity entity)
                {
                    Tags.Remove(entity);
                }
            });
            Initialize();
        }

        public DocumentDetailViewModel(SessionHandler sessionHandler, string documentId) : base(sessionHandler)
        {
            AcceptCommand = new Command(Accept);
            CheckSectionsCommand = new Command(CheckSections);
            RemoveTagCommand = new Command((object o) =>
            {
                if (o is IEntity entity)
                {
                    Tags.Remove(entity);
                }
            });
            Initialize(documentId);
        }

        private async void Initialize(string documentId = "")
        {
            try
            {
                using (Busy())
                {
                    if (Document == null && !string.IsNullOrEmpty(documentId))
                    {
                        _document = new OcrDocument(await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntityById("ocr_document", documentId)));
                    }

                    if (Document.Tags.Count > 0)
                    {
                        var tags = await SessionHandler.ExecuteOpenbiCommand((s, t) =>
                        {
                            var request = s.GetEntities("tag").AddField("Name").AddField("ID").SortAscending("Name");
                            foreach (var tag in Document.Tags)
                            {
                                request.AddFilter("ID", new Filter(tag, FilterOperator.Equal, FilterSign.Include));
                            }
                            return request.Send();
                        });
                        foreach (var tag in tags)
                        {
                            Tags.Add(tag);
                        }
                    }
                    Tags.Add(new Entity(TagTemplateSelector.AddTemplateId));

                    var sections = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntities("ocr_document_section").AddFilter("ocrDocument", new Filter(_document.Id, FilterOperator.Equal)).Send());
                    foreach (var section in sections)
                    {
                        DocumentSections.Add(new OcrDocumentSection(section));
                    }

                    var types = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntities("ocr_document_type").AddField("Name").AddField("ID").Send());
                    var list = new List<IEntity>(types);
                    OnPropertyChanged(nameof(Document));
                    OnPropertyChanged(nameof(CanShowSections));
                    DocumentTypes = list;
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private async void CheckSections(object obj)
        {
            if (await SaveDocument() && DocumentSections.Count > 0)
            {
                ShowSections?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void Accept(object obj)
        {
            if (await SaveDocument())
            {
                Accepted?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task<bool> SaveDocument()
        {
            Document.Tags.Clear();
            Document.Tags = Tags.Select(t => t.Id).ToList();
            using (Busy())
            {
                try
                {
                    await SessionHandler.ExecuteOpenbiCommand((s, t) => s.CreateEntity("ocr_document", Document));
                }
                catch (Exception e)
                {
                    DisplayException(e);
                    return false;
                }
            }
            return true;
        }

    }

    public class TagTemplateSelector : DataTemplateSelector
    {
        public const string AddTemplateId = "91516b74-d349-4e9e-9d2a-f987aee75ce2";

        public DataTemplate TagTemplate { get; set; }
        public DataTemplate AddTagTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is IEntity entity && entity.Id == AddTemplateId)
                return AddTagTemplate;
            return TagTemplate;
        }
    }
}