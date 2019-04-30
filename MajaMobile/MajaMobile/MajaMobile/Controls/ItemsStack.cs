using MajaMobile.Interfaces;
using MajaMobile.Messages;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    class ItemsStack : ScrollView
    {

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsStack), defaultBindingMode: BindingMode.TwoWay, propertyChanged: ItemsSourceChanged);
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsStack));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var itemsLayout = (ItemsStack)bindable;
            itemsLayout.SetItems();
        }

        private void SetItems()
        {
            _content.Children.Clear();

            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                    _content.Children.Add(GetItemView(item));

                if (ItemsSource is INotifyCollectionChanged notifyCollectionChange)
                {
                    notifyCollectionChange.CollectionChanged += CollectionChanged;
                }
            }

        }

        private View GetItemView(object item)
        {
            object content;

            if (ItemTemplate is DataTemplateSelector)
            {
                content = ((DataTemplateSelector)ItemTemplate).SelectTemplate(item, this).CreateContent();
            }
            else
            {
                content = ItemTemplate.CreateContent();
            }

            var view = content as View;
            if (view == null)
                return null;

            view.BindingContext = item;

            view.IsVisible = false;
            view.MeasureInvalidated += View_MeasureInvalidated;

            return view;
        }

        private  void View_MeasureInvalidated(object sender, EventArgs e)
        {
            var view = sender as View;
            if (view != null)
            {
                view.MeasureInvalidated -= View_MeasureInvalidated;
                Device.StartTimer(TimeSpan.FromSeconds(.5), () =>
                {
                    view.IsVisible = true;
                    return false;
                 });
                //uint animationTime = 1000;
                //if (view.BindingContext is ConversationMessage message)
                //{
                //    if (message.Speaker == MajaConversationSpeaker.Maja)
                //    {
                //        view.TranslationX = _deviceInfo.ScreenWidth * -1;
                //    }
                //    else
                //    {
                //        view.TranslationX = _deviceInfo.ScreenWidth;
                //        animationTime = 750;
                //    }
                //}
                //var width = view.Width;
                //if (width > 0)
                //{
                //    view.MeasureInvalidated -= View_MeasureInvalidated;
                //    await view.TranslateTo(0, 0, animationTime, Easing.Linear);
                //}
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex;
                        foreach (var item in e.NewItems)
                            _content.Children.Insert(index++, GetItemView(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        var item = ((IList)ItemsSource)[e.OldStartingIndex];
                        _content.Children.RemoveAt(e.OldStartingIndex);
                        _content.Children.Insert(e.NewStartingIndex, GetItemView(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        _content.Children.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        _content.Children.RemoveAt(e.OldStartingIndex);
                        _content.Children.Insert(e.NewStartingIndex, GetItemView(((IList)ItemsSource)[e.NewStartingIndex]));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _content.Children.Clear();
                    foreach (var item in ItemsSource)
                        _content.Children.Add(GetItemView(item));
                    break;
            }
            if (e.Action == NotifyCollectionChangedAction.Add && _content.Children.Count > 0 && e.NewItems.Count > 0)
            {
                var newitem = (ConversationMessage)e.NewItems[0];
                Device.StartTimer(TimeSpan.FromMilliseconds(750), () =>
                {
                    var child = _content.Children.Last();
                    if (newitem == (ConversationMessage)child.BindingContext)
                    {
                        ScrollToAsync(child, ScrollToPosition.End, true);
                    }
                    return false;
                });
            }
        }


        private StackLayout _content;
        private IDeviceInfo _deviceInfo;

        public ItemsStack()
        {
            Content = _content = new StackLayout { Spacing = 0, Orientation = StackOrientation.Vertical };
            _deviceInfo = DependencyService.Get<IDeviceInfo>();
        }
    }
}