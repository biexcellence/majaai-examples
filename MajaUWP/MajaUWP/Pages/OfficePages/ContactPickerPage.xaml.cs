using MajaUWP.Office;
using MajaUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ContactPickerPage : Page
    {
        public SimpleContact[] contactList;
        public ContactPickerPageViewmodel _viewmodel;
        private MajaConversation _conversation;

        public ContactPickerPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ValueTuple<MajaConversation, string> props)
            {
                _conversation = props.Item1;
                var __contacts = ContactsDeserialization.DeserializeContacts(props.Item2.Remove(0, 8));
                contactList = __contacts;
                DataContext = _viewmodel = new ContactPickerPageViewmodel(contactList);

            }

        }

        private void btn_contact_click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var tb = (TextBlock)btn.Content;
                SimpleContact contact = Array.Find(contactList, c => c.displayName == (string) tb.Text);
                _viewmodel.mailAdress = contact.mailAdress;
            }
              
        }

        private async void EnterButtonClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Frame.GoBack();
            }
            catch (Exception)
            {

            }
            try
            {
                await _conversation.QueryMajaForAnswers(_viewmodel.mailAdress);

            }
            catch (Exception)
            {


            }
        }


        //search
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = contactList.Where(c => c.displayName.Contains(sender.Text));
                _viewmodel.displayContactList = contactList.Where(c => c.displayName.IndexOf(sender.Text)>=0).ToArray();
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            SimpleContact contact = (SimpleContact) args.SelectedItem;
            _viewmodel.mailAdress = contact.mailAdress;
        
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
           
                SimpleContact contact = Array.Find(contactList, c => c.displayName.IndexOf(sender.Text)>=0);
            if (contact != null)
            {
                _viewmodel.mailAdress = contact.mailAdress;
            }
              
            
        }
    }
}

namespace MajaUWP.ViewModels
{
    public class ContactPickerPageViewmodel : ViewModelBase {
        public SimpleContact[] contactList { get; set; }
        private SimpleContact[] _displayContactList { get; set; }
        public SimpleContact[] displayContactList {
            get
            {
                return _displayContactList;
            }
            set {
                _displayContactList = value;
                OnPropertyChanged(nameof(displayContactList));
            }
        }
        public List<string> ContactNames { get; protected set; } = new List<string>();
        private string trueMailAdress { get; set; }
        public string mailAdress
        {
            get
            {
                return trueMailAdress;
            }
            set
            {
                if (value != null)
                {
                    trueMailAdress = value;
                }
                else trueMailAdress = "Kein e-mail adresse hinterlegt";

                OnPropertyChanged(nameof(mailAdress));

            }
        }

        public ContactPickerPageViewmodel(SimpleContact[] cl) {
            contactList = cl;
            displayContactList = contactList.OrderBy(s => s.displayName).ToArray();
            

        }








  


    }

}
