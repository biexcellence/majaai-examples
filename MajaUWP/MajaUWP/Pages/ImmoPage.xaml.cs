using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ImmoPage : Page
    {
        private ImmoPageViewModel _viewModel;

        public ImmoPage()
        {
            InitializeComponent();
            DataContext = _viewModel = new ImmoPageViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IMajaQueryAnswer answer)
            {
                foreach (var immo in (IList<RealEstateObject>)answer.DeserializeData())
                {
                    _viewModel.Immos.Add(immo);
                }
            }
        }

        private void Immo_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is RealEstateObject immo)
            {
                try
                {
                    var uri = new Uri(immo.Href);
                    Frame.Navigate(typeof(BrowserPage), uri);
                }
                catch { }
            }
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class ImmoPageViewModel : ViewModelBase
    {
        public ObservableCollection<RealEstateObject> Immos { get; } = new ObservableCollection<RealEstateObject>();

        public ImmoPageViewModel()
        {

        }
    }
}