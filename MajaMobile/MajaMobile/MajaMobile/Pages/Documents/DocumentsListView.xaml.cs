using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Commands;
using MajaMobile.Utilities;
using Syncfusion.ListView.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public partial class DocumentsListView : ContentView
    {
        private DocumentsListViewModel _viewModel => BindingContext as DocumentsListViewModel;

        public DocumentsListView(DocumentsListViewModel vm)
        {
            InitializeComponent();
            vm.Initialize(SetTagsAndTypes);
        }

        private void SetTagsAndTypes(IEnumerable<IEntity> tags, IEnumerable<IEntity> types)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var converter = Resources["TagConverter"] as TagIdToTagConverter;
                if (converter != null)
                {
                    foreach (var t in tags)
                    {
                        converter.Tags[t.Id] = t;
                    }
                }
                var converter2 = Resources["TypeConverter"] as TypeIdToStringConverter;
                if (converter2 != null)
                {
                    foreach (var t in types)
                    {
                        converter2.Types[t.Id] = t;
                    }
                }
            });
        }

        private async void ListView_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            if (DocumentsList.SelectedItems.Count == 0 && e.ItemData is OcrDocument document && BindingContext is DocumentViewModelBase vm)
            {
                await Navigation.PushAsync(new DocumentDetailPage(vm.SessionHandler, document.Id));
            }
        }

        private void SfListView_SelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            if (DocumentsList.SelectedItems.Count > 0)
            {
                _viewModel.DeleteModeActive = true;
                DocumentsList.SelectionGesture = TouchGesture.Tap;
            }
            else
            {
                _viewModel.DeleteModeActive = false;
                DocumentsList.SelectionGesture = TouchGesture.Hold;
            }
        }

        private void CancelDelete_Clicked(object sender, EventArgs e)
        {
            DocumentsList.SelectedItems.Clear();
            _viewModel.DeleteModeActive = false;
            DocumentsList.SelectionGesture = TouchGesture.Hold;
        }
    }

    public class DocumentsListPage : DocumentPageBase
    {
        public ICommand DeleteCommand { get; }

        public DocumentsListPage(SessionHandler sessionHandler) : base(new DocumentsListViewModel(sessionHandler))
        {
            DeleteCommand = new Command(DeleteDocument);
            OnPropertyChanged(nameof(DeleteCommand));
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void DeleteDocument(object obj)
        {
            if (obj is SfListView listView)
            {
                if (await DisplayAlert("Löschen", "Wollen Sie die ausgewählten Dokumente löschen?", "Ja", "Nein"))
                {
                    listView.SelectionGesture = TouchGesture.Hold;
                    if (ViewModel is DocumentsListViewModel vm)
                    {
                        vm.DeleteModeActive = false;
                        var list = new List<OcrDocument>();
                        foreach (var item in listView.SelectedItems)
                        {
                            if (item is OcrDocument document)
                                list.Add(document);
                        }
                        vm.DeleteDocuments(list);
                    }
                }
            }
        }

        protected override View GetView()
        {
            return new DocumentsListView(ViewModel as DocumentsListViewModel);
        }
    }

    public class DocumentsListViewModel : DocumentViewModelBase
    {
        public ObservableCollection<OcrDocument> Documents { get; } = new ObservableCollection<OcrDocument>();
        private int? _maxItems;
        private Action<IEnumerable<IEntity>, IEnumerable<IEntity>> _setTags;
        private bool _tagsLoaded;

        public bool DeleteModeActive
        {
            get => GetField<bool>();
            set { SetField(value); OnPropertyChanged(nameof(DeleteModeInactive)); }
        }

        public bool DeleteModeInactive => !DeleteModeActive;

        public ICommand LoadMoreCommand { get; }

        public string SearchText
        {
            get => GetField<string>();
            set
            {
                SetField(value);
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (value == SearchText)
                    {
                        _maxItems = null;
                        Documents.Clear();
                        LoadDocuments();
                    }
                    return false;
                });
            }
        }

        public DocumentsListViewModel(SessionHandler sessionHandler) : base(sessionHandler)
        {
            LoadMoreCommand =new Command(LoadDocuments, () => IsIdle && !_maxItems.HasValue || Documents.Count < _maxItems.Value);
        }

        public void Initialize(Action<IEnumerable<IEntity>, IEnumerable<IEntity>> setTags)
        {
            _setTags = setTags;
            LoadDocuments();
        }

        private async void LoadDocuments()
        {
            if (IsBusy || (_maxItems.HasValue && Documents.Count>=_maxItems.Value))
                return;
            try
            {
                using (Busy())
                {

                    if (!_tagsLoaded)
                    {
                        var tags = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntities("tag").AddField("Name").AddField("ID").AddFilter("organisation", new Filter(SessionHandler.Organisation?.Id, FilterOperator.Equal)).Send());
                        var types = await SessionHandler.ExecuteOpenbiCommand((s, t) => s.GetEntities("ocr_document_type").AddField("Name").AddField("ID").Send());
                        _setTags.Invoke(tags, types);
                        _tagsLoaded = true;
                    }

                    var documents = await SessionHandler.ExecuteOpenbiCommand((s, t) =>
                    {
                        var req = s.GetEntities("ocr_document")
                            .AddField("ID")
                            .AddField("Name")
                            .AddField("Tags")
                            .AddField("OcrDocumentType")
                            .AddField("CHANGED")
                            .AddField("CREATED");
                        if (!string.IsNullOrEmpty(SearchText))
                        {
                            req.AddFilter("Name", new Filter(SearchText, FilterOperator.Contains));
                        }
                        req.AddFilter("organisation", new Filter(SessionHandler.Organisation?.Id, FilterOperator.Equal));
                        req.SortDescending("Changed").Offset(Documents.Count).Count(6);
                        return req.Send();
                    });

                    _maxItems = documents.TotalCount;

                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        foreach (var doc in documents)
                        {
                            Documents.Add(new OcrDocument(doc));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        public async void DeleteDocuments(IEnumerable<OcrDocument> documents)
        {
            try
            {
                using (Busy())
                {
                    foreach (var document in documents)
                    {
                        await SessionHandler.ExecuteOpenbiCommand((s, t) => s.DeleteOcrDocument(document.Id));
                        _maxItems--;
                        Documents.Remove(document);
                    }
                }
            }
            catch (Exception e)
            {
                DisplayException(e);
            }
        }
    }

    public class TagIdToTagConverter : IValueConverter
    {
        public Dictionary<string, IEntity> Tags { get; } = new Dictionary<string, IEntity>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> list)
            {
                var tags = new List<IEntity>();
                foreach (var tag in list)
                {
                    if (Tags.TryGetValue(tag, out var t))
                    {
                        tags.Add(t);
                    }
                }
                return tags;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TypeIdToStringConverter : IValueConverter
    {
        public Dictionary<string, IEntity> Types { get; } = new Dictionary<string, IEntity>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (Types.TryGetValue(s, out var t))
                {
                    return t.ToString();
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}