using MajaUWP.Office;
using MajaUWP.ViewModels;
using Newtonsoft.Json;
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
    public sealed partial class CalendarPage : Page
    {
        CalendarEvent calendarEvent;
        CalendarPageViewModel _viemodel;

        public CalendarPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string json)
            {
                var _event = CalendarDeserialization.DeserializeCalendarEvent(json.Remove(0,8));
                calendarEvent = _event;
                DataContext = _viemodel = new CalendarPageViewModel(_event,this.webView);
               
                
            }

        }

    }
}

namespace MajaUWP.ViewModels
{
    public class CalendarPageViewModel : ViewModelBase
    {

        CalendarEvent calendarEvent;
        public string Subject { get;set; }
        public string Body { get; set; }
        public string Location { get;set; }
        public string Subtitle { get; set; }
        WebView web;

        public CalendarPageViewModel(CalendarEvent _calendarEvent, WebView _web)
        {
            calendarEvent = _calendarEvent;
            web = _web;
            SetSubject();
            SetBody();
            SetSubtitle();

        }

        private void SetSubtitle()
        {
            if (calendarEvent.start.dateTime != null && calendarEvent.end.dateTime != null)
            {
                Subtitle += calendarEvent.start.dateTime.ToLocalTime().ToString();
                Subtitle += " bis ";
                Subtitle += calendarEvent.end.dateTime.ToLocalTime().ToString();
            }
            else Subtitle = "";
            if (!string.IsNullOrEmpty(calendarEvent.location.displayName))
            {
                Subtitle += "\r\n Ort: " + calendarEvent.location.displayName;
            }
            OnPropertyChanged(nameof(Subtitle));
        }



        private void SetBody()
        {
            if (!string.IsNullOrEmpty(calendarEvent.body.content))
            {
               
                Body = calendarEvent.body.content;

               
            }
            else Body = "Keine Angaben verfügbar";

            Body += "<!DOCTYPE html><html><head><style>@font-face { font-family: 'segoeui';  src: url('segoeui.ttf'); } html, body {margin: 0; padding: 0; font-size: 20px; background-color: #ededed; font-family: 'segoeui'; color: #000000;}</style></head><body>" +
                                "</body></html>";


            web.NavigateToString(Body);
            
        }

        private void SetSubject()
        {
            if (!string.IsNullOrEmpty(calendarEvent.subject)) Subject = calendarEvent.subject;
            else Subject = "Kein Betreff vorhanden";
            OnPropertyChanged(nameof(Subject));
            
            
        }

        
    }

        
}
