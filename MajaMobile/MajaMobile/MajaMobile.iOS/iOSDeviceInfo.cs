using UIKit;
using MajaMobile.Interfaces;
using Foundation;
using System.Drawing;
using System;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(MajaMobile.iOS.iOSDeviceInfo))]
namespace MajaMobile.iOS
{
    class iOSDeviceInfo : IDeviceInfo
    {

        public iOSDeviceInfo() { }

        public double ScreenHeight
        {
            get
            {
                return UIScreen.MainScreen.Bounds.Height;
            }
        }

        public double ScreenWidth
        {
            get
            {
                return UIScreen.MainScreen.Bounds.Width;
            }
        }

        public double StatusBarHeight
        {
            get
            {
                return UIApplication.SharedApplication.StatusBarFrame.Height;
            }
        }

        public double NavigationBarHeight
        {
            get
            {
                //TODO: get correct height
                return 65.0;
            }
        }

        public string VersionName
        {
            get
            {
                var ver = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"];
                return ver.ToString();
            }
        }

        public string VersionCode
        {
            get
            {
                var ver = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"];
                return ver.ToString();
            }
        }

        public string getLocalFilePath(string filename)
        {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder))
            {
                Directory.CreateDirectory(libFolder);
            }

            return Path.Combine(libFolder, filename);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(getLocalFilePath(filename));
        }
    }
}