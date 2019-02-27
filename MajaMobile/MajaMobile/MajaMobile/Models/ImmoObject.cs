using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Models
{
    public class ImmoObject
    {
        public const string TappedMessage = "IMMO_OBJECT_TAPPED";
        public ICommand TappedCommand { get; }

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
            TappedCommand = new Command(() => MessagingCenter.Send(this, TappedMessage));
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