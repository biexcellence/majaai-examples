using MajaMobile.Extensions;
using MajaMobile.Utilities;
using Sharpnado.Shades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class CustomComboBox : Grid
    {

        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomComboBox));
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(CustomComboBox));
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(CustomComboBox), defaultBindingMode: BindingMode.TwoWay);
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly BindableProperty ResetPossibleProperty = BindableProperty.Create(nameof(ResetPossible), typeof(bool), typeof(CustomComboBox));
        public bool ResetPossible
        {
            get { return (bool)GetValue(ResetPossibleProperty); }
            set { SetValue(ResetPossibleProperty, value); }
        }

        public ICommand TappedCommand { get; }
        private Label _textLabel;
        private Label _placeHolderLabel;
        private Label _caretDownLabel;

        public const int ComboBoxHeight = 60;

        public event EventHandler<CustomSelectionChangedEventArgs> SelectionChanged;

        public CustomComboBox()
        {
            HeightRequest = ComboBoxHeight;
            ColumnSpacing = 0;
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            _caretDownLabel = new Label();
            _caretDownLabel.FontFamily = ((OnPlatform<string>)Application.Current.Resources["FontAwesomeSolid"]).GetRuntimePlatformValue();
            _caretDownLabel.Text = FontAwesome.FontAwesomeIcons.CaretDown;
            _caretDownLabel.FontSize = Device.GetNamedSize(NamedSize.Large, _caretDownLabel);
            _caretDownLabel.HorizontalOptions = LayoutOptions.End;
            _caretDownLabel.VerticalOptions = LayoutOptions.End;
            _caretDownLabel.TextColor = Color.FromHex("#539f90");
            _caretDownLabel.Margin = new Thickness(8, 0, 16, 8);
            SetColumn(_caretDownLabel, 1);
            Children.Add(_caretDownLabel);

            TappedCommand = new Command(ControlTapped);
            var rec = new TapGestureRecognizer();
            rec.BindingContext = this;
            rec.SetBinding(TapGestureRecognizer.CommandProperty, new Binding(nameof(TappedCommand)));
            GestureRecognizers.Add(rec);

            var underline = new BoxView() { HeightRequest = 0.5, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.End, BackgroundColor = ColorScheme.TextColor, Margin = new Thickness(4, 0) };
            SetColumnSpan(underline, 2);
            Children.Add(underline);

            _textLabel = new Label();
            _textLabel.LineBreakMode = LineBreakMode.TailTruncation;
            _textLabel.HorizontalOptions = LayoutOptions.Start;
            _textLabel.VerticalOptions = LayoutOptions.End;
            _textLabel.TextColor = ColorScheme.TextColor;
            _textLabel.Margin = new Thickness(15, 0, 0, 8);
            Children.Add(_textLabel);

            _placeHolderLabel = new Label();
            _placeHolderLabel.BindingContext = this;
            _placeHolderLabel.SetBinding(Label.TextProperty, new Binding(nameof(Placeholder), BindingMode.OneWay));
            _placeHolderLabel.HorizontalOptions = LayoutOptions.Start;
            _placeHolderLabel.VerticalOptions = LayoutOptions.Start;
            _placeHolderLabel.TextColor = ColorScheme.TextColor;
            _placeHolderLabel.FontSize =12;
            _placeHolderLabel.Margin = new Thickness(15, 4, 0, 0);
            Children.Add(_placeHolderLabel);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == ResetPossibleProperty.PropertyName && ResetPossible)
            {
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                var style = (Style)Application.Current.Resources["EditButton"];
                var button = new Button() { Style = style, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End };
                button.CornerRadius = 5;
                button.Margin = new Thickness(6, 0, 0, 0);
                button.HeightRequest = 28;
                button.WidthRequest = 32;
                button.Text = FontAwesome.FontAwesomeIcons.Times;
                button.Command = new Command(ResetSelection);
                SetColumn(button, 2);
                Children.Add(button);
            }
            else if (propertyName == SelectedItemProperty.PropertyName)
            {
                if (SelectedItem != null && !(SelectedItem is string s && string.IsNullOrEmpty(s)))
                {
                    _textLabel.Text = SelectedItem.ToString();
                    _placeHolderLabel.IsVisible = true;
                }
                else
                {
                    _textLabel.Text = Placeholder;
                    _placeHolderLabel.IsVisible = false;
                }
            }
            else if (propertyName == PlaceholderProperty.PropertyName)
            {
                if (SelectedItem == null || (SelectedItem is string s && string.IsNullOrEmpty(s)))
                {
                    _textLabel.Text = Placeholder;
                    _placeHolderLabel.IsVisible = false;
                }
            }
        }


        private Grid _dropdownGrid;
        private Grid _backgroundGrid;
        private ListView _listView;

        private Grid getParentGrid()
        {
            var parent = Parent;
            while (parent != null && !(parent is Grid))
            {
                parent = parent.Parent;
            }
            return parent as Grid;
        }

        protected virtual void ControlTapped()
        {
            var parentGrid = getParentGrid();
            if (ItemsSource != null && ItemsSource.Count > 0 && parentGrid != null)
            {
                _backgroundGrid = new Grid() { BindingContext = this, BackgroundColor = ColorScheme.OverlayColor };
                _backgroundGrid.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(BackgroundTapped) });
                if (parentGrid.RowDefinitions.Count > 1)
                    SetRowSpan(_backgroundGrid, parentGrid.RowDefinitions.Count);
                if (parentGrid.ColumnDefinitions.Count > 1)
                    SetColumnSpan(_backgroundGrid, parentGrid.ColumnDefinitions.Count);
                parentGrid.Children.Add(_backgroundGrid);

                _dropdownGrid = new Grid() { BindingContext = ItemsSource, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
                var shadow = new Shadows() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
                shadow.Shades = new List<Shade>() { new Shade() { BlurRadius = 3, Opacity = 0.6, Offset = new Point(0, 0), Color = ColorScheme.ShadowColor } };
                _dropdownGrid.Children.Add(shadow);

                var grid = new Grid() { BackgroundColor = Color.White, Padding = new Thickness(16, 4, 4, 4) };
                _listView = new ListView() { BackgroundColor = Color.Transparent, SelectionMode = ListViewSelectionMode.None, SeparatorVisibility = SeparatorVisibility.None };
                _listView.ItemTemplate = new DataTemplate(() =>
                {
                    var cell = new ViewCell();
                    var label = new Label() { TextColor = ColorScheme.TextColor };
                    label.SetBinding(Label.TextProperty, new Binding("."));
                    cell.View = label;
                    return cell;
                });
                _listView.SetBinding(ListView.ItemsSourceProperty, new Binding("."));
                _listView.ItemTapped += ListView_ItemTapped;
                grid.Children.Add(_listView);

                shadow.Content = grid;

                var width = Width;
                var height = Math.Min(parentGrid.Width, parentGrid.Height) * 0.8;
                _dropdownGrid.WidthRequest = width;
                _dropdownGrid.HeightRequest = height;
                var x = X;
                var y = Y + Height;

                if (x + width >= parentGrid.Width)
                {
                    x = parentGrid.Width - width - 16;
                }

                _dropdownGrid.TranslationX = x;
                _dropdownGrid.TranslationY = y;

                if (parentGrid.RowDefinitions.Count > 1)
                    SetRowSpan(_dropdownGrid, parentGrid.RowDefinitions.Count);

                if (parentGrid.ColumnDefinitions.Count > 1)
                    SetColumnSpan(_dropdownGrid, parentGrid.ColumnDefinitions.Count);
                parentGrid.Children.Add(_dropdownGrid);
            }
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                var oldItem = SelectedItem;
                SelectedItem = e.Item;
                BackgroundTapped(e.Item);
                SelectionChanged?.Invoke(this, new CustomSelectionChangedEventArgs(e.Item));
            }
        }

        private void BackgroundTapped(object item)
        {
            if (_backgroundGrid != null)
            {
                _backgroundGrid.BindingContext = null;
                if (_backgroundGrid.Parent is Grid grid)
                {
                    grid.Children.Remove(_backgroundGrid);
                }
                _backgroundGrid = null;
            }
            if (_dropdownGrid != null)
            {
                _dropdownGrid.BindingContext = null;
                if (_dropdownGrid.Parent is Grid parent)
                {
                    parent.Children.Remove(_dropdownGrid);
                }
                _dropdownGrid = null;
            }
            if (_listView != null)
            {
                _listView.ItemTapped -= ListView_ItemTapped;
            }
            if (item != null)
                SelectedItem = item;

        }

        protected virtual void ResetSelection()
        {
            SelectedItem = null;
        }
    }

    public class CustomSelectionChangedEventArgs : EventArgs
    {
        public object NewItem { get; }

        public CustomSelectionChangedEventArgs(object newItem)
        {
            NewItem = newItem;
        }
    }
}