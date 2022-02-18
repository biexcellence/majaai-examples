using System.Linq;
using Xamarin.Forms;
namespace MajaMobile.Extensions
{
 public static   class GeneralExtensions
    {
        public static T GetRuntimePlatformValue<T>(this OnPlatform<T> onPlatform)
        {
            foreach (var platform in onPlatform.Platforms)
            {
                if (platform.Platform.FirstOrDefault() == Device.RuntimePlatform)
                {
                    return (T)platform.Value;
                }
            }
            return default;
        }
    }
}