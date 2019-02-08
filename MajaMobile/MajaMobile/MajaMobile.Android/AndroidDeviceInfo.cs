using Android.App;
using Android.Graphics;
using Android.OS;
using MajaMobile.Interfaces;
using Plugin.CurrentActivity;
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

        public byte[] TakeScreenshot(int x, int y, int width, int height)
        {
            Activity activity = CrossCurrentActivity.Current.Activity;
            var view = activity.Window.DecorView;
            view.DrawingCacheEnabled = true;
            view.BuildDrawingCache();
            var bitmap = view.DrawingCache;
            Rect rect = new Rect();
            activity.Window.DecorView.GetWindowVisibleDisplayFrame(rect);
            int statusBarHeight = rect.Top;
            var density = Application.Context.Resources.DisplayMetrics.Density;
            var screenShotBitmap = Bitmap.CreateBitmap(bitmap, (int)(x * density), (int)(y * density) + statusBarHeight, (int)(width * density), (int)(height * density));
            var scaledScreenShotBitmap = Bitmap.CreateScaledBitmap(screenShotBitmap, screenShotBitmap.Width / 2, screenShotBitmap.Height / 2, true);
            view.DestroyDrawingCache();
            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                scaledScreenShotBitmap.Compress(Bitmap.CompressFormat.Webp, 100, stream);
                bitmapData = stream.ToArray();
            }
            screenShotBitmap.Recycle();
            if (screenShotBitmap != scaledScreenShotBitmap && !scaledScreenShotBitmap.IsRecycled)
                scaledScreenShotBitmap.Recycle();
            return bitmapData;
        }

        public void Vibrate()
        {
            var vib = CrossCurrentActivity.Current.Activity.GetSystemService(Android.Content.Context.VibratorService) as Vibrator;
            if (vib != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    vib.Vibrate(VibrationEffect.CreateOneShot(50, VibrationEffect.DefaultAmplitude));
                else
                    vib.Vibrate(50);
            }
        }
    }
}