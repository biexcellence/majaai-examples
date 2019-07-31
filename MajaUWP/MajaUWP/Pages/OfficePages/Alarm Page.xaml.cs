using GalaSoft.MvvmLight.Messaging;
using MajaUWP.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Alarm_Page : Page
    {

        MajaConversation maja;
        private AlarmPageViewmodel viewModel;

        public Alarm_Page()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            

            if (e.Parameter is ValueTuple<MajaConversation, bool> props)
            {
                DataContext = viewModel = new AlarmPageViewmodel(props.Item2,new ListView[3] {this.ListViewHours,this.ListViewMinutes,this.ListViewSecounds});
                maja = props.Item1;
            }
        }
        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!viewModel.IsAlarm)
            {
                if (viewModel.Secounds != 0 | viewModel.Minutes != 0 | viewModel.Hours != 0)
                {
                    DateTimeOffset alarmTime = DateTimeOffset.Now;

                    alarmTime = alarmTime.AddHours(viewModel.Hours);
                    alarmTime = alarmTime.AddMinutes(viewModel.Minutes);
                    alarmTime = alarmTime.AddSeconds(viewModel.Secounds);

                    Messenger.Default.Send<(string, DateTimeOffset, string)>( ("alarm", alarmTime, "Timer abgelaufen"));


                    //MainPage.setAlarm(alarmTime, "Timer abgelaufen!!");
                    
                    this.Frame.GoBack();
                    maja.Messages.Add(new MajaConversationMessage($"Timer klingelt in {viewModel.Hours}h {viewModel.Minutes}m {viewModel.Secounds}s"));
                }
                else
                {
                    var dialog = new MessageDialog("Der Timer muss länger als 0 Sekunden gehen");
                    var result = await dialog.ShowAsync();
                    
                }
            }
            else
            {
                DateTimeOffset alarmDate = new DateTimeOffset(viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day, viewModel.Hours, viewModel.Minutes, viewModel.Secounds, TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.Now));
                if (alarmDate.ToUnixTimeMilliseconds() - DateTimeOffset.Now.ToUnixTimeMilliseconds() >0)
                {
                    
                    Messenger.Default.Send<(string, DateTimeOffset, string)>(("alarm", alarmDate, "Wecker klingelt!!"));

                    TimeSpan offset = alarmDate.Subtract(DateTimeOffset.Now);

                    this.Frame.GoBack();
                    string majaMessage = "";
                    if (offset.Days != 0) majaMessage += offset.Days + "t ";
                    if (offset.Hours != 0) majaMessage += offset.Hours + "h ";
                    if (offset.Minutes != 0) majaMessage += offset.Minutes + "m ";
                    if (offset.Seconds != 0) majaMessage += offset.Seconds + "s";
                    
                    maja.Messages.Add(new MajaConversationMessage($"Der Wecker klingelt in {majaMessage}"));
                }
                else
                {
                    var dialog = new MessageDialog("Der Wecker muss in der Zukunft liegen! (Geht die Uhr von OfficeMaja richtig?)");
                    var result = await dialog.ShowAsync();
                }


            }
            



        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListView lv)
            {
                lv.ScrollIntoView(lv.SelectedItem);
            }

        }

        private void ListView_ScrollSelectionIntoView(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            lv.ScrollIntoView(lv.SelectedItem);
        }
    }




}
namespace MajaUWP.ViewModels
{
    public class AlarmPageViewmodel : ViewModelBase
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Secounds { get; set; }
        bool _isAlarm { get; set; }
        public DateTimeOffset Date { get; set; }
        public bool IsAlarm
        {
            get
            { return _isAlarm; }
            set
            {
                _isAlarm = value;
                OnPropertyChanged(nameof(IsAlarm));
                if (value)
                {
                    Description = "Der Wecker klingelt um:";
                }
                else
                {
                    Description = "Der Timer klingelt in:";
                    Hours = 0;
                    Minutes = 0;
                    Secounds = 0;
                    OnPropertyChanged(nameof(Hours));
                    OnPropertyChanged(nameof(Minutes));
                    OnPropertyChanged(nameof(Secounds));
                }
                OnPropertyChanged(nameof(Description));
                
            }
        }
        public string Description { get; set; }
        public int[] SixtyList { get;set; }
        public int[] TwentyFourList { get; set; }
        public ListView[] listViews { get; set; }

        public AlarmPageViewmodel(bool isalarm_, ListView[] listViews_) {
            IsAlarm = isalarm_;
            listViews = listViews_;
            Date = DateTimeOffset.Now;

            //setting The timepicker to now;
            if (IsAlarm)
            {
                var now = DateTime.Now;
                Hours = now.Hour;
                OnPropertyChanged(nameof(Hours));
                Minutes = now.Minute;
                OnPropertyChanged(nameof(Minutes));
                Secounds = now.Second;
                OnPropertyChanged(nameof(Secounds));
            }

            Date = DateTimeOffset.Now;
            OnPropertyChanged(nameof(Date));

            SixtyList = new int[60];
            for (int i = 0; i < 60; i++)
            {
                SixtyList[i] = i;
            }
            OnPropertyChanged(nameof(SixtyList));
            TwentyFourList = new int[24];
            for (int i = 0; i < 24; i++)
            {
                TwentyFourList[i] = i;
            }
            OnPropertyChanged(nameof(TwentyFourList));
        }

    }
}

namespace MajaUWP.Converters
{
    public class InvertBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}
