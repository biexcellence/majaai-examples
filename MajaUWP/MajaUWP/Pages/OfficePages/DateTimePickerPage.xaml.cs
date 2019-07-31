using MajaUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class DateTimePickerPage : Page
    {
        private string selectedDate { get;set; }
        private string selectedTime { get; set; }
        private string selectedDateTime { get; set; }
        MajaConversation _conversation;
        DateTimePickerViewmodel _viewmodel;




        public DateTimePickerPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is MajaConversation conv)
            {
                _conversation = conv;
            }
            DataContext = _viewmodel = new DateTimePickerViewmodel(_conversation.Messages.Last().Text);

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //date
            DateTimeOffset date = _viewmodel.date;

            string day = date.Day.ToString();
            if (day.Length == 1) day = "0" + day;

            string month = date.Month.ToString();
            if (month.Length == 1) month = "0" + month;

            selectedDate = day + "." + month + "." + date.Year.ToString().Remove(0, 2);

            //time
            TimeSpan time = _viewmodel.time;
            selectedTime = time.ToString().Substring(0, 5);



            selectedDateTime = selectedDate + " " + selectedTime;


            try
            {
                this.Frame.GoBack();
            }
            catch (Exception)
            {

            }
            try
            {
                await _conversation.QueryMajaForAnswers(selectedDateTime);

            }
            catch (Exception)
            {


            }

        }
    }
}

namespace MajaUWP.ViewModels
{
public class DateTimePickerViewmodel : ViewModelBase
    {
        public TimeSpan time { get;set; }
        public DateTimeOffset date { get;set; }
       
        public string Text { get; set; }

        public DateTimePickerViewmodel(string _text)
        {
            Text = _text;
            time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
            date = new DateTimeOffset(DateTime.Now);
        }

    }

}
