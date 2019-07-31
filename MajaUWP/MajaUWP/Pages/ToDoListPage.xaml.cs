using MajaUWP.Office;
using MajaUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static MajaUWP.Office.TodoItem;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace MajaUWP.Pages
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ToDoListPage : Page
    {
        ToDoListPageViewmodel viewmodel;
        TodoHandler tdh = new TodoHandler();
        int currentSelection = -1;

        public ToDoListPage()
        {
            this.InitializeComponent();
            
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var TodoList = await tdh.loadList();
            DataContext = viewmodel = new ToDoListPageViewmodel(TodoList);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            await tdh.saveTodoList(viewmodel.TodoList);
        }

        private void ItemTapped(object sender, TappedRoutedEventArgs e)
        {
           
            var textblock = sender as TextBlock;
            string text = textblock.Text;

            TodoItem todoItem = Array.Find(viewmodel.TodoList.ToArray(), c => c.Text== text);

            int index = viewmodel.TodoList.IndexOf(todoItem);
            todoItem.IsDone = !todoItem.IsDone;
            viewmodel.TodoList[index] = todoItem;
            
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox;
            TextBox inputTextBox;
            ContentDialog dialog;
            ShowAddItemDialog(out comboBox, out inputTextBox, out dialog);
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                UrgencyStates state = ConvertStringToUrgency(comboBox.SelectedValue);
                if (!string.IsNullOrEmpty(inputTextBox.Text))
                {
                    viewmodel.TodoList.Add(new TodoItem(inputTextBox.Text, state));
                }
            }
        }



        private UrgencyStates ConvertStringToUrgency(object selectedValue)
        {
            switch (selectedValue)
            {
                case "Niedrig":
                    return UrgencyStates.lowPriority;
                case "Hoch":
                    return UrgencyStates.highPriority;
                default:
                    return UrgencyStates.normalPriority;
            }
        }


        private void AllDoneButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < viewmodel.TodoList.Count; i++)
            {
                TodoItem item = viewmodel.TodoList[i];
                item.IsDone = true;
                viewmodel.TodoList[i] = item;
            }
        }

        private void AllNoteDoneButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < viewmodel.TodoList.Count; i++)
            {
                TodoItem item = viewmodel.TodoList[i];
                item.IsDone = false;
                viewmodel.TodoList[i] = item;
            }
        }

        private void SortPriorityAscButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList = new ObservableCollection<TodoItem>(viewmodel.TodoList.OrderBy((s) => s.Urgency));
        }
        private void SortPriorityDesButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList = new ObservableCollection<TodoItem>(viewmodel.TodoList.OrderBy((s) => s.Urgency).Reverse());
        }
        private void SortDoneDesButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList = new ObservableCollection<TodoItem>(viewmodel.TodoList.OrderBy((s) => s.IsDone).Reverse());
        }
        private void SortDoneAscButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList = new ObservableCollection<TodoItem>(viewmodel.TodoList.OrderBy((s) => s.IsDone));
        }
        

        private static void ShowEditDialog(out ComboBox comboBox, out TextBox inputTextBox, out ContentDialog dialog, TodoItem toEdit
            )
        {
            List<string> urgencyList = new List<string>();
            urgencyList.Add("Niedrig");
            urgencyList.Add("Normal");
            urgencyList.Add("Hoch");

            //Grid
            Grid grid = new Grid();

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();

            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);



            //Combobox
            comboBox = new ComboBox();
            comboBox.ItemsSource = urgencyList;
            comboBox.PlaceholderText = "Priorität";
            comboBox.HorizontalAlignment = HorizontalAlignment.Center;
            comboBox.Margin = new Thickness(10);
            comboBox.SelectedIndex = (int)toEdit.Urgency;
            Grid.SetRow(comboBox, 1);
            grid.Children.Add(comboBox);

            //TextBox
            inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            inputTextBox.Margin = new Thickness(10);
            inputTextBox.PlaceholderText = "Titel";
            inputTextBox.Text = toEdit.Text;
            Grid.SetRow(inputTextBox, 0);
            grid.Children.Add(inputTextBox);


            //Dialog
            dialog = new ContentDialog();
            dialog.Content = grid;
            dialog.Title = "Aufgabe hinzufügen";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "abbrechen";
        }
        private static void ShowAddItemDialog(out ComboBox comboBox, out TextBox inputTextBox, out ContentDialog dialog)
        {
            List<string> urgencyList = new List<string>();
            urgencyList.Add("Niedrig");
            urgencyList.Add("Normal");
            urgencyList.Add("Hoch");

            //Grid
            Grid grid = new Grid();

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();

            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);



            //Combobox
            comboBox = new ComboBox();
            comboBox.ItemsSource = urgencyList;
            comboBox.PlaceholderText = "Priorität";
            comboBox.HorizontalAlignment = HorizontalAlignment.Center;
            comboBox.Margin = new Thickness(10);
            Grid.SetRow(comboBox, 1);
            grid.Children.Add(comboBox);

            //TextBox
            inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            inputTextBox.Margin = new Thickness(10);
            inputTextBox.PlaceholderText = "Titel";
            Grid.SetRow(inputTextBox, 0);
            grid.Children.Add(inputTextBox);


            //Dialog
            dialog = new ContentDialog();
            dialog.Content = grid;
            dialog.Title = "Aufgabe hinzufügen";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "abbrechen";
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            currentSelection = -1;
            AppBarButton appBarButton = sender as AppBarButton;
            Grid grid = appBarButton.Parent as Grid;
            TextBlock textBlock = grid.Children[0] as TextBlock;
            string itemText = textBlock.Text;

            TodoItem todoItem = Array.Find(viewmodel.TodoList.ToArray(), c => c.Text == itemText);
            int index = viewmodel.TodoList.IndexOf(todoItem);
            currentSelection = index;
        }

        private void DeleteAllButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList = new ObservableCollection<TodoItem>();
        }

        private void DeleteDoneButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList = new ObservableCollection<TodoItem>(viewmodel.TodoList.OrderBy((s) => s.IsDone).Reverse());

            List<TodoItem> newList = new List<TodoItem>();
            foreach (var item in viewmodel.TodoList)
            {
               
                if (!item.IsDone)
                {
                    newList.Add(item);
                }
            }
            viewmodel.TodoList = new ObservableCollection<TodoItem>(newList);      

        }

        private async void ContextEditButton_Clicked(object sender, RoutedEventArgs e)
        {
            TodoItem toEdit = viewmodel.TodoList[currentSelection];
            ComboBox comboBox;
            ContentDialog dialog;
            TextBox textBox;
            ShowEditDialog(out comboBox, out textBox, out dialog,toEdit);
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {

                UrgencyStates state = ConvertStringToUrgency(comboBox.SelectedValue);

                TodoItem item = viewmodel.TodoList[currentSelection];
                item.Urgency = state;
                item.Text = textBox.Text??"Kein Titel festgelegt";
                viewmodel.TodoList[currentSelection] = item;
            }
        }
        private void ContextDeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            viewmodel.TodoList.RemoveAt(currentSelection);
        }
    }
}
namespace MajaUWP.ViewModels
{
    public class ToDoListPageViewmodel : ViewModelBase

    {
        

        private ObservableCollection<TodoItem> _todoList { get; set; }
        public ObservableCollection<TodoItem> TodoList { get
            { return _todoList; } set {
                _todoList = value;
                OnPropertyChanged(nameof(TodoList));
            } }

        public ToDoListPageViewmodel(ObservableCollection<TodoItem> _todoList)
        {
            TodoList = _todoList;
            OnPropertyChanged(nameof(TodoList));
        }
        public void propertyChanged(object todoThisTo)
        {
            OnPropertyChanged(nameof(todoThisTo));
        }
    }
}
namespace MajaUWP.Converters
{
    public class ConvertUrgencyToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UrgencyStates state)
            {
                switch (state)
                {
                    case UrgencyStates.lowPriority:
                        return "#51c64f";
                    case UrgencyStates.normalPriority:
                        return "#c6b533";
                    case UrgencyStates.highPriority:
                        return "#c92434";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ConvertISDONTETOStrikeThrough : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isDone && isDone)
            {
                return TextDecorations.Strikethrough;
            }
            else
            {
                return TextDecorations.None;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ConvertIsDoneToFontColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isDone && isDone)
            {
                return "#919396";
            }
            else
            {
                return "#000000";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}