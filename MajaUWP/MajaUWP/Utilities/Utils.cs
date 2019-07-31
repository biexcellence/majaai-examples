using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace MajaUWP.Utilities
{
    public class Utils
    {
        public const string MajaApiKey = "";
        public const string MajaApiSecret = "";
        public static List<string> DefaultPackages { get; set; } = new List<string>() { "BIEXCELLENCE", "MajaAI", "MajaFun", "CORE", "WIKIPEDIA", "f96d2d65-cd3e-4ebc-8166-e2b0eedd0119", "9583f23c-fc4a-46b9-9ffa-fdbfef4565d5" };
        public static List<string> MajaPackages

        {
            get
            {
                List<string> packageList = JsonConvert.DeserializeObject<List<string>>(AppSettingHandler.GetAppSetting("MajaPackages").ToString());

                if (packageList == null)
                {
                    return new List<string>();
                }
                return packageList;
            }
            set
            {
               string jsonString = JsonConvert.SerializeObject(value);
                AppSettingHandler.SetAppSetting("MajaPackages", jsonString);
            }


        }

        public const string MicrosoftClientId  = "";
        public static string microSoftToken { get;set; }
        public static string[] MicrosoftPermissionScopes { get; set; } = {"user.read", "calendars.readwrite", "notes.create", "contacts.readwrite", "people.read", "Mail.Send" };
        public static bool IsMajaLoggedIn { get; set; }

        public static void SetTokenExpirationTimer(DateTimeOffset expired) {
            DateTime utcExpired = expired.UtcDateTime.ToUniversalTime();
            DateTime now = DateTime.UtcNow.ToUniversalTime();
            TimeSpan ts = (utcExpired - now);

            DispatcherTimer alarmTimer = new DispatcherTimer { Interval = ts};
            alarmTimer.Tick += (s, e) => {
                alarmTimer.Stop();
                microSoftToken = "";
               
            };
            alarmTimer.Start();
        }

        public static void AddPackage(string toAdd)
        {
            var packageList = MajaPackages;
            packageList.Add(toAdd);
            MajaPackages = packageList;
        }
        public static void AddDefaultPackages()
        {
            foreach (var package in DefaultPackages)
            {
                if (!MajaPackages.Contains(package))
                {
                    AddPackage(package);
                }

            }
        }

        public static void RemovePackage(string toRemoveId) {
            var packageList = MajaPackages;
            packageList.Remove(toRemoveId);
            MajaPackages = packageList;
        }
    }
}
