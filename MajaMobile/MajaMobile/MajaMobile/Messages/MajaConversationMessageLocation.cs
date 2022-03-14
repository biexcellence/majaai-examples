using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Collections.Generic;
using System.Globalization;
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

        public List<Location> Locations { get; } = new List<Location>();

        private string _image;
        public override string Image
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
                if (entity.DisplayAttributes.TryGetValue("lat", out var lat) && entity.DisplayAttributes.TryGetValue("lon", out var @long))
                {
                    Locations.Add(new Location((double)lat, (double)@long, entity.Name));
                }
            }
        }

        protected override void MessageTapped()
        {
            MessagingCenter.Send(this, ConversationMessageTappedMessage);
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
                parameters.Add("zoom=9");
            }
            parameters.Add("key="); //TODO:Your Google Maps key
            url += string.Join("&", parameters);
            return url;
        }
    }
}