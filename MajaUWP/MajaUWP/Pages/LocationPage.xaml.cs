using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class LocationPage : MajaPage
    {
        public LocationPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is object[] arr)
            {
                try
                {
                    Geopoint geoPoint = new Geopoint(new BasicGeoposition() { Latitude = (double)arr[0], Longitude = (double)arr[1] });
                    MapIcon pin = new MapIcon { Location = geoPoint, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = (string)arr[2], ZIndex = 0 };
                    Map.MapElements.Add(pin);
                    Map.Center = geoPoint;
                    Map.ZoomLevel = 12;
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message);
                }
            }
        }
    }
}