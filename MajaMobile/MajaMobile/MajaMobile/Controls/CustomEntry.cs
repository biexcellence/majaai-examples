using MajaMobile.Behaviors;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class CustomEntry : ContentView
    {
        private Entry _entry;

        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CustomEntry));
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomEntry), defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty CompletedCommandProperty = BindableProperty.Create(nameof(CompletedCommand), typeof(ICommand), typeof(CustomEntry));
        public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(CustomEntry));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

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

        public Keyboard Keyboard
        {
            get { return (Keyboard)GetValue(KeyboardProperty); }
            set { SetValue(KeyboardProperty, value); }
        }

        public bool IsPassword
        {
            get => _entry.IsPassword;
            set => _entry.IsPassword = value;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == PlaceholderProperty.PropertyName)
            {
                _entry.Placeholder = Placeholder;
            }
            else if (propertyName == KeyboardProperty.PropertyName)
            {
                _entry.Keyboard = Keyboard;
            }
        }

        public CustomEntry()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    _entry = new IOSEntry();
                    break;
                default:
                    _entry = new DroidEntry();
                    break;
            }
            _entry.Keyboard = Keyboard.Default;
            _entry.BindingContext = this;
            _entry.HorizontalTextAlignment = TextAlignment.Start;
            _entry.SetBinding(Entry.TextProperty, new Binding(nameof(Text), BindingMode.TwoWay));
            var behavior = new EventToCommandBehavior() { EventName = nameof(Entry.Completed) };
            behavior.SetBinding(EventToCommandBehavior.CommandProperty, new Binding(nameof(CompletedCommand)));
            _entry.Behaviors.Add(behavior);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    _entry.HeightRequest = 50;
                    Content = _entry;
                    break;
                default:
                    Content = new Frame() { HasShadow = true, CornerRadius = 5.0f, Padding = new Thickness(10, 5, 10, 0), BackgroundColor = Utilities.ColorScheme.EntryBackgroundColor, Content = _entry };
                    break;
            }
        }

        public void FocusEntry()
        {
            _entry.Focus();
        }

        public void UnfocusEntry()
        {
            _entry.Unfocus();
        }
    }
}