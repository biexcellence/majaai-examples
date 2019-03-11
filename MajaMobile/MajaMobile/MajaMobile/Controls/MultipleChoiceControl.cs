using BiExcellence.OpenBi.Api.Commands.MajaAi;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class MultipleChoiceControl : ScrollView
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ChatInputControl));
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsStack), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ItemsSourceChanged);
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private StackLayout _content;

        public MultipleChoiceControl()
        {
            Orientation = ScrollOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.Fill;
            Content = _content = new StackLayout { Spacing = 0, Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand };
        }

        private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var itemsLayout = (MultipleChoiceControl)bindable;
            if (newValue is INotifyCollectionChanged notifyCollection)
                itemsLayout.SetCollection(notifyCollection);
        }

        private void SetCollection(INotifyCollectionChanged notifyCollection)
        {
            _content.Children.Clear();
            notifyCollection.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    IsVisible = false;
                    _content.Children.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                    IsVisible = true;
                    foreach (var item in e.NewItems)
                    {
                        if (item is IPossibleUserReply reply)
                        {
                            var frame = new Frame() { HasShadow = false, BorderColor = Color.LightGray, CornerRadius = 30, Padding = new Thickness(10, 6), Margin = new Thickness(5, 0, 0, 0), BackgroundColor = Color.White, HorizontalOptions = LayoutOptions.Start };
                            var label = new Label() { Text = reply.Text, HorizontalTextAlignment = TextAlignment.Center, FontFamily = "seguisbi.ttf#seguisbi" };
                            if (Device.RuntimePlatform == Device.iOS)
                            {
                                frame.CornerRadius = 18;
                                label.FontFamily = "SegoeUI-SemiboldItalic";
                                label.FontSize = 18;
                            }
                            frame.Content = label;
                            var recognizer = new TapGestureRecognizer();
                            recognizer.SetBinding(TapGestureRecognizer.CommandProperty, new Binding(nameof(Command)));
                            recognizer.CommandParameter = reply;
                            recognizer.BindingContext = this;
                            frame.GestureRecognizers.Add(recognizer);
                            _content.Children.Add(frame);

                            //var button = new Button() { Text = reply.Text, HorizontalOptions = LayoutOptions.Start };
                            //button.BindingContext = this;
                            //button.SetBinding(Button.CommandProperty, new Binding(nameof(Command)));
                            //button.CommandParameter = reply;
                            //_content.Children.Add(button);
                        }
                    }
                    break;
            }
        }
    }
}