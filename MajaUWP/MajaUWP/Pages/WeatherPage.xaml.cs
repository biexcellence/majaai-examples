using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Models;
using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using MajaUWP.WeatherControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class WeatherPage : Page
    {
        private WeatherViewModel _viewModel;

        public WeatherPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is WeatherForecast forecast)
            {
                DataContext = _viewModel = new WeatherViewModel(forecast);
            }
        }

        private bool _animationRunning;
        private bool _isSwiped;
        private async void Weather_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial && !_isSwiped && !_animationRunning)
            {
                var distance = e.Cumulative.Translation.X;
                if (Math.Abs(distance) <= 2)
                    return;

                _isSwiped = true;
                var translateTransform = CurrentWeatherGrid.RenderTransform = new TranslateTransform();
                Storyboard storyboard = new Storyboard();

                DoubleAnimationUsingKeyFrames animationKeyFrames = new DoubleAnimationUsingKeyFrames();

                var keyFrame = new LinearDoubleKeyFrame();
                keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(750));
                keyFrame.Value = distance > 0 ? ActualWidth : -ActualWidth;
                animationKeyFrames.KeyFrames.Add(keyFrame);

                keyFrame = new LinearDoubleKeyFrame();
                keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(751));
                keyFrame.Value = distance > 0 ? -ActualWidth : ActualWidth;
                animationKeyFrames.KeyFrames.Add(keyFrame);

                keyFrame = new LinearDoubleKeyFrame();
                keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1500));
                keyFrame.Value = 0;
                animationKeyFrames.KeyFrames.Add(keyFrame);

                Storyboard.SetTargetProperty(animationKeyFrames, "X");
                Storyboard.SetTarget(animationKeyFrames, translateTransform);
                storyboard.Children.Add(animationKeyFrames);

                storyboard.Completed += Animation_Completed;
                _animationRunning = true;
                storyboard.Begin();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                 {
                     await Task.Delay(751);
                     if (distance > 0)
                         _viewModel.SwitchDay(false);
                     else
                         _viewModel.SwitchDay(true);
                 });
            }
        }

        private void Animation_Completed(object sender, object e)
        {
            if (sender is Storyboard s)
            {
                s.Completed -= Animation_Completed;
            }
            _animationRunning = false;
        }

        private void Weather_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            _isSwiped = false;
        }

        private async void Weather_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement f && f.DataContext is WeatherWrapper weather)
            {
                if (!_animationRunning)
                {
                    var scaleTransform = CurrentWeatherGrid.RenderTransform = new ScaleTransform();
                    Storyboard storyboard = new Storyboard();
                    DoubleAnimationUsingKeyFrames animationKeyFrames = new DoubleAnimationUsingKeyFrames();
                    var keyFrame = new LinearDoubleKeyFrame();
                    keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1));
                    keyFrame.Value = 0.1;
                    animationKeyFrames.KeyFrames.Add(keyFrame);
                    keyFrame = new LinearDoubleKeyFrame();
                    keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500));
                    keyFrame.Value = 1;
                    animationKeyFrames.KeyFrames.Add(keyFrame);
                    Storyboard.SetTargetProperty(animationKeyFrames, "ScaleX");
                    Storyboard.SetTarget(animationKeyFrames, scaleTransform);
                    storyboard.Children.Add(animationKeyFrames);

                    var animationKeyFramesY = new DoubleAnimationUsingKeyFrames();
                    keyFrame = new LinearDoubleKeyFrame();
                    keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1));
                    keyFrame.Value = 0.1;
                    animationKeyFramesY.KeyFrames.Add(keyFrame);
                    keyFrame = new LinearDoubleKeyFrame();
                    keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500));
                    keyFrame.Value = 1;
                    animationKeyFramesY.KeyFrames.Add(keyFrame);
                    Storyboard.SetTargetProperty(animationKeyFramesY, "ScaleY");
                    Storyboard.SetTarget(animationKeyFramesY, scaleTransform);
                    storyboard.Children.Add(animationKeyFramesY);

                    storyboard.Completed += Animation_Completed;
                    _animationRunning = true;
                    storyboard.Begin();

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(50);
                        _viewModel.CurrentWeather = weather;
                    });
                }
            }
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class WeatherViewModel : ViewModelBase
    {
        public ICommand DayTappedCommand { get; }
        public List<WeatherWrapper> Forecast { get; }

        private WeatherWrapper _currentWeather;
        public WeatherWrapper CurrentWeather
        {
            get => _currentWeather;
            set
            {
                if (_currentWeather != null)
                    _currentWeather.IsSelected = false;
                value.IsSelected = true;
                _currentWeather = value;
                OnPropertyChanged();
            }
        }
        public string Time { get; }
        public string City { get; }
        public WeatherViewModel(WeatherForecast forecast)
        {
            Forecast = forecast.Select(w => new WeatherWrapper(w)).ToList();
            CurrentWeather = Forecast.FirstOrDefault();
            City = forecast.City;
            Time = DateTime.Now.ToString("HH:00");
            DayTappedCommand = new Command(DayTapped);
        }

        private void DayTapped(object obj)
        {
            if (obj is WeatherWrapper weather)
            {
                CurrentWeather = weather;
            }
        }

        public void SwitchDay(bool goToNext)
        {
            var index = Forecast.IndexOf(CurrentWeather);
            if (goToNext)
                index++;
            else
                index--;
            if (index < 0)
                index = Forecast.Count - 1;
            else if (index == Forecast.Count)
                index = 0;
            CurrentWeather = Forecast[index];
        }
    }
}
namespace MajaUWP.Models
{
    public class WeatherWrapper : PropertyChangedOnMainThread
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
        public WeatherDetails Weather { get; }

        public WeatherWrapper(WeatherDetails weather)
        {
            Weather = weather;
        }
    }
}
namespace MajaUWP.Converters
{
    public class WeatherControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is WeatherDetails weather)
            {
                if (weather.SymbolNumber >= 200 && weather.SymbolNumber < 300)
                {
                    return new ThunderstormControl();
                }
                else if (weather.SymbolNumber >= 300 && weather.SymbolNumber < 400)
                {
                    if (weather.IsNightTime)
                        return new NightRainShowersControl();
                    return new RainShowersControl();
                }
                else if (weather.SymbolNumber >= 400 && weather.SymbolNumber < 500)
                {
                    return "Unknown weather";
                }
                else if (weather.SymbolNumber >= 500 && weather.SymbolNumber < 600)
                {
                    return new RainyControl();
                }
                else if (weather.SymbolNumber >= 600 && weather.SymbolNumber < 700)
                {
                    return new SnowyControl();
                }
                else if (weather.SymbolNumber >= 700 && weather.SymbolNumber < 800)
                {
                    return new FoggyControl();
                }
                else if (weather.SymbolNumber >= 800 && weather.SymbolNumber < 900)
                {
                    switch (weather.SymbolNumber)
                    {
                        case 800 when weather.IsNightTime:
                            return new MoonControl();
                        case 800:
                            return new SunControl();
                        case 801 when weather.IsNightTime:
                            return new NightPartlyCloudyControl();
                        case 801:
                            return new PartlyCloudyControl();
                        case 802 when weather.IsNightTime:
                        case 803 when weather.IsNightTime:
                            return new NightPartlyCloudyExtendedControl();
                        case 802:
                        case 803:
                            return new PartlyCloudyExtendedControl();
                        default:
                            if (weather.IsNightTime)
                                return new CloudyGrayControl();
                            return new CloudyWhiteControl();
                    }
                }
            }
            return "Unknown weather";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateToDayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime date)
                if (parameter == null)
                    return DateTimeFormatInfo.CurrentInfo.GetDayName(date.DayOfWeek);
                else
                    return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(date.DayOfWeek);
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class WindAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int dir)
                return (double)Math.Abs(dir - 180);
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HasValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Visibility.Collapsed;
            if (value is double d && d.Equals(default(double)))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Windows.UI.Colors.LightSlateGray);
            return new SolidColorBrush(Windows.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}