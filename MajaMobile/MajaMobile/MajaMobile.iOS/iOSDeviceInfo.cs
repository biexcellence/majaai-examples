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

        public byte[] TakeScreenshot(int x, int y, int width, int height)
        {
            var image = UIScreen.MainScreen.Capture();

            var imgSize = image.Size;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, width, height);
            context.ClipToRect(clippedRect);
            var drawRect = new RectangleF(-x, -(y + 20), (float)imgSize.Width, (float)imgSize.Height);
            image.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            byte[] array;
            using (var imageData = modifiedImage.AsJPEG())
            {
                array = new byte[imageData.Length];
                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, array, 0, Convert.ToInt32(imageData.Length));
            }
            return array;
        }

        public void Vibrate()
        {
            AudioToolbox.SystemSound.Vibrate.PlayAlertSound();
        }
    }
}