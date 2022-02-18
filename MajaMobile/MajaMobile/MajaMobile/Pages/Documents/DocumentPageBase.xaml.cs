using BiExcellence.OpenBi.Api;
using BiExcellence.OpenBi.Api.Commands.Entities;
using MajaMobile.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages.Documents
{
    public abstract partial class DocumentPageBase : ContentPage
    {
        public DocumentViewModelBase ViewModel { get; protected set; }
        private bool _pageActive;
        protected abstract View GetView();
        private IDocumentPageContent _content;

        public DocumentPageBase(DocumentViewModelBase viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
            Visual = VisualMarker.Material;
            //  NavigationPage.SetHasNavigationBar(this, false);
            var view = GetView();
            if (view is IDocumentPageContent content)
                _content = content;
            PageContent.Content = view;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _pageActive = true;
            if (ViewModel != null)
            {
                ViewModel.GoBackPage += GoBack;
                ViewModel.ShowException += DisplayException;
                ViewModel.SendAppearing();
                if (_content != null)
                    _content.SendAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _pageActive = false;
            if (ViewModel != null)
            {
                ViewModel.GoBackPage -= GoBack;
                ViewModel.ShowException -= DisplayException;
                ViewModel.SendDisappearing();
                if (_content != null)
                    _content.SendDisappearing();
            }
        }

        private void GoBack(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void DisplayException(object sender, ThreadExceptionEventArgs e)
        {
            DisplayException(e.Exception);
        }

        protected void DisplayException(Exception e)
        {
            DisplayException(e, this);
        }

        public static async void DisplayException(Exception e, Page p)
        {
            var handled = false;
            if (e is OpenBiServerErrorException)
            {
                var openBiServerError = (OpenBiServerErrorException)e;
                if (openBiServerError.Response.Code == OpenBiResponseCodes.LoginFailed)
                {
                    await p.DisplayAlert("Fehler", "Benutzername oder Passwort falsch", "Ok");
                    handled = true;
                }
            }
            if (!handled)
            {
                await p.DisplayAlert("Fehler", e.Message, "Ok");
            }
        }

    }

    public interface IDocumentPageContent
    {
        void SendAppearing();
        void SendDisappearing();
    }

    public class DocumentViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<ThreadExceptionEventArgs> ShowException;
        public event EventHandler GoBackPage;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SessionHandler SessionHandler { get; }
        public ICommand GoBackCommand { get; }

        public bool NotifyChecked
        {
            get => GetField<bool>();
            set
            {
                var old = NotifyChecked;
                if (old != value)
                {
                    SetField(value);
                }
            }
        }

        public DocumentViewModelBase(SessionHandler sessionHandler)
        {
            GoBackCommand = new Command(GoBack);
            SessionHandler = sessionHandler;
        }

        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        protected T GetField<T>([CallerMemberName] string caller = null)
        {
            object value = null;
            if (_fields.TryGetValue(caller, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        protected void SetField(object value, [CallerMemberName] string caller = "", bool raiseChanged = true)
        {
            _fields[caller] = value;
            if (raiseChanged)
                OnPropertyChanged(caller);
        }

        protected void GoBack()
        {
            GoBackPage?.Invoke(this, EventArgs.Empty);
        }

        public void DisplayException(Exception ex)
        {
            Device.BeginInvokeOnMainThread(() => ShowException?.Invoke(this, new ThreadExceptionEventArgs(ex)));
        }

        public virtual void Dispose()
        {

        }

        public virtual void SendAppearing()
        {

        }

        public virtual void SendDisappearing()
        {

        }

        #region "IsBusy"
        private int _busyCounter;
        public bool IsIdle => !IsBusy;

        public bool IsBusy => _busyCounter > 0;

        public IDisposable Busy()
        {
            Increment();

            return new DelegateDisposable(Decrement);

            void Increment()
            {
                Interlocked.Increment(ref _busyCounter);
                RaisEvents();
            }

            void Decrement()
            {
                Interlocked.Decrement(ref _busyCounter);
                RaisEvents();
            }

            void RaisEvents()
            {
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsIdle));
            }
        }

        private class DelegateDisposable : IDisposable
        {
            private readonly Action _action;

            public DelegateDisposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
        #endregion
    }

    public class ComboboxEntityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string id && values[1] is IEnumerable<IEntity> entities)
                return entities.FirstOrDefault(e => e.Id == id);
            else if (values[0] is string bId && values[1] is IEnumerable<IBaseEntity> baseEntities)
                return baseEntities.FirstOrDefault(e => e.Id == bId);
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is IEntity entity)
            {
                return new[] { entity.Id };
            }
            if (value is IBaseEntity baseEntity)
            {
                return new[] { baseEntity.Id };
            }
            return null;
        }
    }

    public class ComboboxBaseEntityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string id && values[1] is IEnumerable<IBaseEntity> entities)
                return entities.FirstOrDefault(e => e.Id == id);
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is IBaseEntity entity)
            {
                return new[] { entity.Id };
            }
            return null;
        }
    }
}