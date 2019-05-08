using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Models;
using MajaUWP.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
                foreach (var entity in answer.Entities.Where(en => string.Equals(en.EntityProvider, "immobilienProvider", StringComparison.OrdinalIgnoreCase)))
                {
                    _viewModel.Immos.Add(new ImmoObject(entity));
                }
            }
        }

        private void Immo_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ImmoObject immo)
            {
                try
                {
                    var uri = new Uri(immo.Link);
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
        public ObservableCollection<ImmoObject> Immos { get; } = new ObservableCollection<ImmoObject>();

        public ImmoPageViewModel()
        {

        }
    }
}

namespace MajaUWP.Models
{
    public class ImmoObject
    {
        public string PurchasePrice { get; }
        public string Id { get; }
        public string VendorId { get; }
        public string Name { get; }
        public double Price { get; }
        public double ColdRent { get; }
        public double NetColdRent { get; }
        public double WarmRent { get; }
        public int Rooms { get; }
        public double LivingSpace { get; }
        public string Image { get; }
        public string City { get; }

        public string Link
        {
            get
            {
                return $"http://iframe.immort.de/expose/{Id}?anbieternr={VendorId}&showkontaktform=X";
            }
        }

        public ImmoObject(IMajaEntity entity)
        {
            object obj;
            if (entity.DisplayAttributes.TryGetValue("OBJEKTTITEL", out obj))
            {
                Name = (string)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("ID", out obj))
            {
                Id = (string)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("ANBIETERNR", out obj))
            {
                VendorId = (string)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("PRICE", out obj) && obj != null)
            {
                Price = (double)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("KAUFPREIS", out obj) && obj != null)
            {
                var price = (double)obj;
                if (price == 0)
                    PurchasePrice = "Preis auf Anfrage";
                else
                    PurchasePrice = string.Format("{0:N0} €", price);
            }
            if (entity.DisplayAttributes.TryGetValue("KALTMIETE", out obj) && obj != null)
            {
                ColdRent = (double)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("NETTOKALTMIETE", out obj) && obj != null)
            {
                NetColdRent = (double)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("WARMMIETE", out obj) && obj != null)
            {
                WarmRent = (double)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("ANZAHL_ZIMMER", out obj) && obj != null)
            {
                Rooms = Convert.ToInt32(obj);
            }
            if (entity.DisplayAttributes.TryGetValue("WOHNFLAECHE", out obj) && obj != null)
            {
                LivingSpace = (double)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("PREVIEWIMAGE", out obj))
            {
                if (!string.IsNullOrEmpty((string)obj))
                    Image = "http:" + (string)obj;
            }
            if (entity.DisplayAttributes.TryGetValue("GEO_ORT", out obj))
            {
                City = (string)obj;
            }
        }
    }
}