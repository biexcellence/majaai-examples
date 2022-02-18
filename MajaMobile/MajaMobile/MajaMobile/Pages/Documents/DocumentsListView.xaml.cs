using BiExcellence.OpenBi.Api.Commands;
using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Commands;
using MajaMobile.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Extended;

namespace MajaMobile.Pages.Documents
{
    public partial class DocumentsListView : ContentView
    {
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

    }

    public class DocumentsListPage : DocumentPageBase
    {
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }

        public DocumentsListPage(SessionHandler sessionHandler) : base(new DocumentsListViewModel(sessionHandler))
        {
            DeleteCommand = new Command(DeleteDocument);
            EditCommand = new Command(EditDocument);
        }

        private async void EditDocument(object obj)
        {
            if (obj is OcrDocument document)
            {
                await Navigation.PushAsync(new DocumentDetailPage(ViewModel.SessionHandler, document.Id));
            }
        }

        private async void DeleteDocument(object obj)
        {
            if (obj is OcrDocument document)
            {
                if (await DisplayAlert("Löschen", "Wollen Sie das Dokument: " + document.Name + " löschen?", "Ja", "Nein"))
                {
                    if (ViewModel is DocumentsListViewModel vm)
                    {
                        vm.DeleteDocument(document);
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
        public InfiniteScrollCollection<OcrDocument> Documents { get; } = new InfiniteScrollCollection<OcrDocument>();
        private int? _maxItems;
        private Action<IEnumerable<IEntity>, IEnumerable<IEntity>> _setTags;
        private bool _tagsLoaded;

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
                        Documents.LoadMoreAsync();
                    }
                    return false;
                });
            }
        }

        public DocumentsListViewModel(SessionHandler sessionHandler) : base(sessionHandler)
        {
            Documents.OnLoadMore = async () =>
            {
                using (Busy())
                {
                    return await LoadDocuments();
                }
            };
            Documents.OnCanLoadMore = () => !_maxItems.HasValue || Documents.Count < _maxItems.Value;
        }

        public void Initialize(Action<IEnumerable<IEntity>, IEnumerable<IEntity>> setTags)
        {
            _setTags = setTags;
            Documents.LoadMoreAsync();
        }

        private async Task<IEnumerable<OcrDocument>> LoadDocuments(bool reset = false)
        {
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
                    var list = new List<OcrDocument>();

                    foreach (var doc in documents)
                    {
                        list.Add(new OcrDocument(doc));
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            return new List<OcrDocument>();
        }

        public async void DeleteDocument(OcrDocument document)
        {
            try
            {
                await SessionHandler.ExecuteOpenbiCommand((s, t) => s.DeleteOcrDocument(document.Id));
                _maxItems--;
                Documents.Remove(document);
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