using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Models
{
    public class PointOfInterest
    {
        public const string TappedMessage = "POI_TAPPED";
        public ICommand TappedCommand { get; }

        public string Id { get; }
        public string Name { get; }
        public string Level { get; }
        public double Rating { get; }
        public double RatingLocal { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public string NameSuffix { get; }
        public string OriginalName { get; }
        public string Url { get; }
        //{
        //    get
        //    {
        //        return $"https://maps.sygic.com/#/?item={Id}&map=13,{Latitude.ToString(CultureInfo.InvariantCulture)},{Longitude.ToString(CultureInfo.InvariantCulture)}";
        //    }
        //}
        public string Marker { get; }
        public string Perex { get; }
        public string Image { get; }

        public PointOfInterest(IMajaEntity entity)
        {
            TappedCommand = new Command(() => MessagingCenter.Send(this, TappedMessage));
            object obj;
            Id = entity.Id;
            Name = entity.Name;
            if (entity.DisplayAttributes.TryGetValue("level", out obj))
                Level = (string)obj;
            if (entity.DisplayAttributes.TryGetValue("rating", out obj) && obj != null)
                Rating = (double)obj;
            if (entity.DisplayAttributes.TryGetValue("rating_local", out obj) && obj != null)
                RatingLocal = (double)obj;
            if (entity.DisplayAttributes.TryGetValue("lat", out obj) && obj != null)
                Latitude = (double)obj;
            if (entity.DisplayAttributes.TryGetValue("lng", out obj) && obj != null)
                Longitude = (double)obj;
            if (entity.DisplayAttributes.TryGetValue("name_suffix", out obj))
                NameSuffix = (string)obj;
            if (entity.DisplayAttributes.TryGetValue("url", out obj))
                Url = (string)obj;
            if (entity.DisplayAttributes.TryGetValue("original_name", out obj))
                OriginalName = (string)obj;
            if (entity.DisplayAttributes.TryGetValue("marker", out obj))
                Marker = (string)obj;
            if (entity.DisplayAttributes.TryGetValue("perex", out obj))
                Perex = (string)obj;
            if (entity.DisplayAttributes.TryGetValue("thumbnail_url", out obj))
                Image = (string)obj;
        }
    }
}