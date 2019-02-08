using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageLocation : MajaConversationMessage
    {
        public class Location
        {
            public double Lat { get; }
            public double Long { get; }
            public string Name { get; }
            public Location(double lat, double @long, string name)
            {
                Lat = lat;
                Long = @long;
                Name = name;
            }
        }

        public const string LocationTappedMessage = "LOCATION_TAPPED";
        public ICommand LocationTappedCommand { get; }
        public List<Location> Locations { get; } = new List<Location>();

        private string _image;
        public string Image
        {
            get
            {
                if (_image == null)
                    _image = GetMapImageUrl();
                return _image;
            }
        }

        public MajaConversationMessageLocation(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            foreach (var entity in queryAnswer.Entities)
            {
                if (entity.CustomAttributes.TryGetValue("lat", out var lat) && entity.CustomAttributes.TryGetValue("lon", out var @long))
                {
                    Locations.Add(new Location((double)lat, (double)@long, entity.Name));
                }
            }
            LocationTappedCommand = new Command(() => MessagingCenter.Send(this, LocationTappedMessage));
        }

        private string GetMapImageUrl()
        {
            CultureInfo info = new CultureInfo("en-US");
            string url = "https://maps.googleapis.com/maps/api/staticmap?";

            var parameters = new List<string>();
            foreach (var location in Locations)
            {
                parameters.Add("markers=size:small|" + location.Lat.ToString(info) + "," + location.Long.ToString(info));
            }
            parameters.Add("size=1000x500");
            parameters.Add("maptype=roadmap");
            parameters.Add("scale=2");
            parameters.Add("format=png");
            if (Locations.Count == 1)
            {
                parameters.Add("zoom=7");
            }
            parameters.Add("key=AIzaSyA8So_RPU4ipGFa1Qc9GZv3JaZjYlxHG5k");
            url += string.Join("&", parameters);
            return url;
        }

        public string GetMapUrl()
        {
            CultureInfo info = new CultureInfo("en-US");
            var url = "";
            //TODO: other locations
            var location = Locations[0];
            var name = Uri.EscapeDataString(location.Name);

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    url = $"geo:{location.Lat.ToString(info)},{location.Long.ToString(info)}?q={name}";
                    break;
                case Device.iOS:
                    url = "http://maps.apple.com/?ll=" + location.Lat.ToString(info) + "," + location.Long.ToString(info) + "&q=" + name;
                    break;
            }
            return url;
        }
    }
}