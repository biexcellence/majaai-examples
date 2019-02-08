﻿namespace MajaMobile.Interfaces
{
    public interface IDeviceInfo
    {
        double ScreenWidth { get; }
        double ScreenHeight { get; }
        double StatusBarHeight { get; }
        double NavigationBarHeight { get; }
        string VersionName { get; }
        string VersionCode { get; }
        byte[] TakeScreenshot(int x, int y, int width, int height);
        string getLocalFilePath(string filename);
        bool FileExists(string filename);
        void Vibrate();
    }
}