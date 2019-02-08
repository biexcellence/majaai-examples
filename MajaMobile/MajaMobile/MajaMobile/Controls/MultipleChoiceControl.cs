using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaMobile.Messages;
using System.Collections;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class MultipleChoiceControl : StackLayout
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsStack), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ItemsSourceChanged);
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public MultipleChoiceControl()
        {
            Orientation = StackOrientation.Vertical;
        }

        private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var itemsLayout = (MultipleChoiceControl)bindable;
            itemsLayout.SetItems();
        }

        private void SetItems()
        {
            Children.Clear();
            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    if (item is IPossibleUserReply reply)
                    {
                        var button = new Button() { Text = reply.Text, HorizontalOptions = LayoutOptions.Center };
                        button.SetBinding(Button.CommandProperty, new Binding(nameof(UserConversationMessageMultipleChoice.ReplyTappedCommand)));
                        button.CommandParameter = reply;
                        Children.Add(button);
                    }
                }
                if (ItemsSource is INotifyCollectionChanged notifyCollectionChange)
                {
                    notifyCollectionChange.CollectionChanged += CollectionChanged;
                }
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    if (sender is INotifyCollectionChanged notifyCollectionChange)
                    {
                        notifyCollectionChange.CollectionChanged -= CollectionChanged;
                    }
                    IsVisible = false;
                    Children.Clear();
                    break;
            }
        }
    }
}