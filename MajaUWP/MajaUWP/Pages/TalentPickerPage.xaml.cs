using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Utilities;
using MajaUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public sealed partial class TalentPickerPage : Page
    {
        TalentPickerViewmodel viewmodel; 
        public TalentPickerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            DataContext = viewmodel = new TalentPickerViewmodel(cvs, TalentListView,LoadingAnimationPanel, TalentAutoSuggestionBox, PublicTalentsCheckbox);
        }


        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            
            if (TalentListView.SelectedItems.Count != 0)
            {
                e.Cancel = true;

                ContentDialog dialog;

                TextBlock tb = new TextBlock();

                tb.Text = "Möchten Sie die Ausgewählten Talente übernehmen? \r\n";

                foreach (var talent in TalentListView.SelectedItems)
                {
                    if (talent is Talent t)
                    {
                        tb.Text += "- " + t.Name + "\r\n";
                    }
                 
                }

                dialog = new ContentDialog();
                dialog.Content = tb;
                dialog.Title = "Auswahl übernehmen";
                //dialog.IsSecondaryButtonEnabled = true;
                
                dialog.PrimaryButtonText = "OK";
                //dialog.SecondaryButtonText = "abbrechen";
                dialog.CloseButtonText = "abbrechen";

                dialog.PrimaryButtonClick += ContentAcceptBtn_Clicked;

                await dialog.ShowAsync();
                
            }

            base.OnNavigatingFrom(e);
           

        }


        private void ContentAcceptBtn_Clicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            InstallSelectedPackages();
            TalentListView.SelectedItems.Clear();
            this.Frame.GoBack();
        }

        private void TalentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewmodel.SelectedCount = TalentListView.SelectedItems.Count;
        }

        private void AcceptButton_click(object sender, RoutedEventArgs e)
        {
            InstallSelectedPackages();
            TalentListView.SelectedItems.Clear();
            this.Frame.GoBack();
        }

        private void InstallSelectedPackages()
        {
            foreach (var package in TalentListView.SelectedItems)
            {
                Talent t = package as Talent;
                Utils.AddPackage(t.Id);
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (TalentListView.Visibility == Visibility.Visible && sender.Text != "")
                {
                    List<Talent> list;
                    if (viewmodel.ShowPublicTalents) { list = viewmodel.TalentListAllTalents; }
                    else { list = viewmodel.TalentListPrivateTalents; }
                    

                    sender.ItemsSource = list.Where(t => t.Name.Contains(sender.Text));
                    TalentListView.ScrollIntoView(list.Where(t => t.Name.Contains(sender.Text)).FirstOrDefault());
                }
                
            }
        }




        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

            TalentListView.SelectedItems.Add(args.ChosenSuggestion);
            sender.Text = "";
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class TalentPickerViewmodel : ViewModelBase
    {
        ListView TalentView;
        StackPanel LoadingPanel;
        AutoSuggestBox suggestBox;

        public List<Talent> TalentListAllTalents { get; set; }
        public List<Talent> TalentListPrivateTalents;
        public IEnumerable<IGrouping<string, Talent>> GroupedListAllTalents { get; set; }
        public IEnumerable<IGrouping<string, Talent>> GroupedListPrivateTalents { get; set; }
        public IEnumerable<IGrouping<string, Talent>> DisplayList { get; set; }
        CollectionViewSource collectionViewSource;
        CheckBox PublicCheckBox;
        int _selectedCount;
        private bool showPublicTalents;

        public int SelectedCount
        {
            get { return _selectedCount; }
            set
            {
                _selectedCount = value;
                OnPropertyChanged(nameof(SelectedCount));
            }
        }
        public bool ShowPublicTalents { get => showPublicTalents;
           set {
                showPublicTalents = value;

                if (value) DisplayList = GroupedListAllTalents;
                else DisplayList = GroupedListPrivateTalents;
                OnPropertyChanged(nameof(DisplayList));
                OnPropertyChanged(nameof(ShowPublicTalents));


                
            }
        }

        public TalentPickerViewmodel(object v, ListView talentView, StackPanel loadingPanel, AutoSuggestBox box, CheckBox checkBox)
        {
            
            TalentView = talentView;
            LoadingPanel = loadingPanel;
            suggestBox = box;
            PublicCheckBox = checkBox;
            collectionViewSource = v as CollectionViewSource;
            SetupTalentList();

        }

        public async void SetupTalentList()
        {
            SessionHandler sessionHandler = new SessionHandler();
            await sessionHandler.LoginWithSavedCredential();
            IList<IMajaTalent> talents = await sessionHandler.ExecuteOpenbiCommand((s, t) => s.GetMajaTalents());
            TalentListAllTalents = new List<Talent>();
            TalentListPrivateTalents = new List<Talent>();

            foreach (var ITalent in talents)
            {
                if (!Utils.MajaPackages.Contains(ITalent.Id))
                {
                    Talent t = new Talent(ITalent, sessionHandler);

                    TalentListAllTalents.Add(t);
                    if (!ITalent.IsPublic)
                    {
                        TalentListPrivateTalents.Add(t);
                    }
                }


            }

            IEnumerable<IGrouping<string, Talent>> groups = from talent in TalentListAllTalents
                                                            group talent by talent.Category.Name;

            IEnumerable<IGrouping<string, Talent>> privateGroups =  from talent in TalentListPrivateTalents
                                                                    group talent by talent.Category.Name;



            GroupedListAllTalents = groups;
            GroupedListPrivateTalents = privateGroups;
            DisplayList = GroupedListAllTalents;
            ShowPublicTalents = true;
            LoadingPanel.Visibility = Visibility.Collapsed;
            TalentView.Visibility = Visibility.Visible;
            suggestBox.Visibility = Visibility.Visible;
            PublicCheckBox.Visibility = Visibility.Visible;
        }

    }
}

namespace MajaUWP.Converters
{
    public class SelectedCountToStringConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int count = (int) value;
            if (count == 0) return "";
            else if (count == 1) return "Ein Talent ausgewählt";
            return count + " Talente ausgewählt";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NullableBooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool?)
            {
                return (bool)value;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return (bool)value;
            return false;
        }
    }
}


