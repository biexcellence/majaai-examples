using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Utilities;
using Syncfusion.SfAutoComplete.XForms;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class ChatInputControl : ContentView
    {
        private View _currentElement;

        public event EventHandler<Syncfusion.SfAutoComplete.XForms.ValueChangedEventArgs> AutoCompleteValueChanged;
        public event EventHandler<Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs> AutoCompleteSelectionChanged;

        public static readonly BindableProperty CurrentUserInputProperty = BindableProperty.Create(nameof(CurrentUserInput), typeof(IPossibleUserReply), typeof(ChatInputControl));
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ChatInputControl), defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty CompletedCommandProperty = BindableProperty.Create(nameof(CompletedCommand), typeof(ICommand), typeof(ChatInputControl));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand CompletedCommand
        {
            get { return (ICommand)GetValue(CompletedCommandProperty); }
            set { SetValue(CompletedCommandProperty, value); }
        }

        public IPossibleUserReply CurrentUserInput
        {
            get { return (IPossibleUserReply)GetValue(CurrentUserInputProperty); }
            set { SetValue(CurrentUserInputProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == CurrentUserInputProperty.PropertyName)
            {
                SetControl();
            }
        }

        private void DisposeCurrentControl()
        {
            if (_currentElement != null)
                _currentElement.Unfocus();
            if (_currentElement is Entry entry)
            {
                entry.RemoveBinding(Entry.TextProperty);
                entry.Completed -= _entry_Completed;
                entry.BindingContext = null;
            }
            else if (_currentElement is DatePicker datePicker)
            {
                datePicker.Unfocused -= _datePicker_Unfocused;
            }
            else if (_currentElement is Slider slider)
            {
                slider.RemoveBinding(Slider.ValueProperty);
                slider.BindingContext = null;
            }
            else if (_currentElement is SfAutoComplete autoComplete)
            {
                autoComplete.SelectionChanged -= AutoComplete_SelectionChanged;
                autoComplete.ValueChanged -= AutoComplete_ValueChanged;
                autoComplete.RemoveBinding(SfAutoComplete.DataSourceProperty);
                autoComplete.BindingContext = null;
            }
            _currentElement = null;
        }

        private T GetControl<T>() where T : View
        {
            if (_currentElement == null || !(_currentElement.GetType().IsSubclassOf(typeof(T))))
            {
                DisposeCurrentControl();
                View element = null;
                if (typeof(T) == typeof(Entry))
                {
                    Entry entry = null;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        entry = new IOSEntry();
                        entry.HeightRequest = 50;
                    }
                    else
                    {
                        entry = new DroidEntry();
                    }
                    entry.BindingContext = this;
                    entry.HorizontalTextAlignment = TextAlignment.Start;
                    entry.SetBinding(Entry.TextProperty, new Binding(nameof(Text), BindingMode.TwoWay));
                    entry.Completed += _entry_Completed;
                    _currentElement = element = entry;
                }
                else if (typeof(T) == typeof(CustomSlider))
                {
                    var slider = new CustomSlider();
                    slider.BindingContext = this;
                    IValueConverter converter = null;
                    if (CurrentUserInput != null && string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Double, StringComparison.OrdinalIgnoreCase))
                        converter = new StringToDoubleConverter();
                    else
                        converter = new StringToIntConverter();
                    slider.SetBinding(Slider.ValueProperty, new Binding(nameof(Text), BindingMode.TwoWay, converter));
                    _currentElement = slider;

                    var grid = new Grid() { ColumnSpacing = 0 };
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    var label = new Label();
                    label.BindingContext = this;
                    label.SetBinding(Label.TextProperty, new Binding(nameof(Text)));
                    Grid.SetColumn(slider, 1);
                    grid.Children.Add(label);
                    grid.Children.Add(slider);
                    element = grid;
                }
                else if (typeof(T) == typeof(DatePicker))
                {
                    var picker = new DatePicker();
                    picker.Unfocused += _datePicker_Unfocused;
                    _currentElement = element = picker;
                }
                else if (typeof(T) == typeof(SfAutoComplete))
                {
                    EntitySearchResults = new ObservableCollection<IMajaEntity>();
                    var autoComplete = new SfAutoComplete() { AutoCompleteMode = AutoCompleteMode.Suggest, SuggestionMode = SuggestionMode.StartsWith, SuggestionBoxPlacement = SuggestionBoxPlacement.Top, ShowBorder = false };
                    autoComplete.Watermark = CurrentUserInput.Text;
                    autoComplete.DisplayMemberPath = nameof(IMajaEntity.Name);
                    autoComplete.SelectionChanged += AutoComplete_SelectionChanged;
                    autoComplete.ValueChanged += AutoComplete_ValueChanged;
                    autoComplete.BindingContext = this;
                    autoComplete.SetBinding(SfAutoComplete.DataSourceProperty, new Binding(nameof(EntitySearchResults)));
                    _currentElement = element = autoComplete;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        element = new Frame() { HasShadow = false, CornerRadius = 5.0f, Padding = new Thickness(0, 6), BackgroundColor = ColorScheme.EntryBackgroundColor, Content = autoComplete };
                    }
                    else
                    {
                        autoComplete.Margin = new Thickness(0, 0, 0, -3);
                    }
                }
                if (Device.RuntimePlatform == Device.iOS)
                    Content = element;
                else
                    ((Frame)Content).Content = element;
            }
            return _currentElement as T;
        }

        private void SetControl()
        {
            if (CurrentUserInput == null)
            {
                var entry = GetControl<Entry>();
                entry.Placeholder = "Nachricht schreiben";
                entry.Keyboard = Keyboard.Default;
            }
            else if (string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Date, StringComparison.OrdinalIgnoreCase))
            {
                GetControl<DatePicker>();
            }
            else if (string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Entity, StringComparison.OrdinalIgnoreCase))
            {
                //always dispose even if current control is SfAutoComplete because of binding to collection
                DisposeCurrentControl();
                GetControl<SfAutoComplete>();
            }
            else
            {
                if (string.Equals(CurrentUserInput.ControlType, PossibleUserReplyControlType.Slider, StringComparison.OrdinalIgnoreCase))
                {
                    //always dispose even if current control is slider because of Double vs Int in binding converter
                    DisposeCurrentControl();
                    var slider = GetControl<CustomSlider>();
                    if (CurrentUserInput.ControlOptions.TryGetValue("HIGH", out var high))
                        slider.Maximum = Convert.ToDouble(high);
                    if (CurrentUserInput.ControlOptions.TryGetValue("LOW", out var low))
                        slider.Minimum = Convert.ToDouble(low);
                    if (CurrentUserInput.ControlOptions.TryGetValue("RANGESTEP", out var rangeStep))
                        slider.StepValue = Convert.ToDouble(rangeStep);
                }
                else
                {
                    var entry = GetControl<Entry>();
                    if (!string.IsNullOrEmpty(CurrentUserInput.Text))
                        entry.Placeholder = CurrentUserInput.Text;
                    else
                        entry.Placeholder = "Nachricht schreiben";
                    if (string.Equals(CurrentUserInput.Type, PossibleUserReplyType.Integer, StringComparison.OrdinalIgnoreCase))
                        entry.Keyboard = Keyboard.Numeric;
                    else
                        entry.Keyboard = Keyboard.Default;
                }
            }
        }

        public ChatInputControl()
        {
            if (!(Device.RuntimePlatform == Device.iOS))
            {
                Content = new Frame() { HasShadow = true, CornerRadius = 5.0f, Padding = new Thickness(10, 1, 10, 4), BackgroundColor = ColorScheme.EntryBackgroundColor };
            }
            SetControl();
        }

        private void _datePicker_Unfocused(object sender, FocusEventArgs e)
        {
            if (sender is DatePicker datePicker)
                CompletedCommand?.Execute(datePicker.Date);
        }

        private void _entry_Completed(object sender, EventArgs e)
        {
            CompletedCommand?.Execute(null);
        }

        #region Slider

        private class StringToIntConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string s && double.TryParse(s, out var d))
                    return d;
                return 0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double d)
                    return ((int)d).ToString();
                return "";
            }
        }

        private class StringToDoubleConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                    return d;
                return 0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double d)
                    return d.ToString(CultureInfo.InvariantCulture);
                return "";
            }
        }
        #endregion

        #region AutoComplete
        public ObservableCollection<IMajaEntity> EntitySearchResults { get; private set; }

        private void AutoComplete_ValueChanged(object sender, Syncfusion.SfAutoComplete.XForms.ValueChangedEventArgs e)
        {
            AutoCompleteValueChanged?.Invoke(this, e);
        }

        private void AutoComplete_SelectionChanged(object sender, Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs e)
        {
            if (e.Value != null)
            {
                AutoCompleteSelectionChanged?.Invoke(this, e);
                CompletedCommand?.Execute(e.Value);
            }
        }

        #endregion
    }

    public class IOSEntry : Entry
    {

    }

    public class DroidEntry : Entry
    {

    }
}