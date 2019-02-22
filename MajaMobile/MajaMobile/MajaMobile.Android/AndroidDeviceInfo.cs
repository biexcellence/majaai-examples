using Android.App;
using Android.Graphics;
using Android.OS;
using MajaMobile.Interfaces;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(MajaMobile.Droid.AndroidDeviceInfo))]
namespace MajaMobile.Droid
{
    public class AndroidDeviceInfo : IDeviceInfo
    {

        public AndroidDeviceInfo() { }

        private double? _ScreenHeight;
        public double ScreenHeight
        {
            get
            {
                if (_ScreenHeight == null)
                    _ScreenHeight = (Application.Context.Resources.DisplayMetrics.HeightPixels) / Application.Context.Resources.DisplayMetrics.Density;
                return _ScreenHeight.Value;
            }
        }

        private double? _ScreenWidth;
        public double ScreenWidth
        {
            get
            {
                if (_ScreenWidth == null)
                    _ScreenWidth = (Application.Context.Resources.DisplayMetrics.WidthPixels) / Application.Context.Resources.DisplayMetrics.Density;
                return _ScreenWidth.Value;
            }
        }

        public double StatusBarHeight
        {
            get
            {
                return 0.0;
            }
        }

        public double NavigationBarHeight
        {
            get
            {
                //TODO: get correct height
                return 60.0;
            }
        }

        public string VersionName
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
            }
        }

        public string VersionCode
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionCode.ToString();
            }
        }

        public string getLocalFilePath(string filename)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return System.IO.Path.Combine(path, filename);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(getLocalFilePath(filename));
        }
    }
}