using MajaUWP.Office;
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
    public sealed partial class MailPage : Page
    {
        Mail mail;
        MailPageViewModel _viewmodel;
        public MailPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string json)
            {
                var _mail = MailDeserialization.DeserializeMail(json.Remove(0, 4));
                mail = _mail;
                DataContext = _viewmodel = new MailPageViewModel(mail, this.webView);



            }

        }
    }
}
namespace MajaUWP.ViewModels
{
    public class MailPageViewModel : ViewModelBase
    {
        Mail mail;
        WebView webView;

        public string Sender { get; set; }
        public string Recipients { get; set; }
        public string Subject{get;set;}
        public string DateSent { get; set; }
        public string DateRecieved { get; set; }
        public MailPageViewModel(Mail _mail, WebView _webView)
        {
            mail = _mail;
            webView = _webView;
            if (!string.IsNullOrEmpty(mail.sender.emailAddress.name)) Sender = mail.sender.emailAddress.name;
            else Sender = "";
            OnPropertyChanged(nameof(Sender));

            foreach (var adress in mail.toRecipients)
            {
                Recipients += adress.emailAddress.name + "; ";
            }
            //if (Recipients.Length > 30) Recipients = Recipients.Substring(0, 30) + "...";
            OnPropertyChanged(nameof(Recipients));

            if (!string.IsNullOrEmpty(mail.subject)) Subject = mail.subject;
            else Subject = "Kein Betreff";
            OnPropertyChanged(nameof(Subject));

            if (!string.IsNullOrEmpty(mail.body.content))
            {
                string content = mail.body.content;
                content += "<!DOCTYPE html><html><head><style>@font-face { font-family: 'segoeui';  src: url('segoeui.ttf'); } html, body {margin: 0; padding: 0; font-size: 50px; background-color: #ededed; font-family: 'segoeui'; color: #000000;}</style></head><body>" + "</body></html>";
                webView.NavigateToString(content);
            }
            else
            {
                webView.NavigateToString("Diese Mail wurde ohne Inhalt versendet");
            }
            

            DateSent = mail.sentDateTime.ToLocalTime().ToString();
            DateSent = DateSent.Remove(DateSent.Length - 3);
            OnPropertyChanged(nameof(DateSent));
            DateRecieved = mail.receivedDateTime.ToLocalTime().ToString();
            DateRecieved = DateRecieved.Remove(DateRecieved.Length - 3);
            OnPropertyChanged(nameof(DateRecieved));

          
        }
       


    }


}

