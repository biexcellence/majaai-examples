using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class ActivityIndicatorLayout : AbsoluteLayout
    {
        public ActivityIndicatorLayout() { }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent != null)
            {
                if (Children.Count == 1)
                {
                    var view = Children[0];
                    SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                    SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
                    view.HorizontalOptions = view.VerticalOptions = LayoutOptions.FillAndExpand;
                }
                Children.Add(getIndicator());
            }
        }

        private AbsoluteLayout getIndicator()
        {
            var layout = new AbsoluteLayout();
            SetLayoutFlags(layout, AbsoluteLayoutFlags.All);
            SetLayoutBounds(layout, new Rectangle(0, 0, 1, 1));
            layout.BackgroundColor = Color.FromHex("#33eeeeee");
            layout.SetBinding(IsVisibleProperty, nameof(ViewModels.ViewModelBase.IsBusy));

            var indicator = new ActivityIndicator();
            SetLayoutFlags(indicator, AbsoluteLayoutFlags.All);
            SetLayoutBounds(indicator, new Rectangle(0.5, 0.5, 1, 1));
            indicator.HorizontalOptions = indicator.VerticalOptions = LayoutOptions.CenterAndExpand;
            indicator.Color = Utilities.ColorScheme.UserMessageColor;
            indicator.SetBinding(IsVisibleProperty, nameof(ViewModels.ViewModelBase.IsBusy));
            indicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ViewModels.ViewModelBase.IsBusy));
            indicator.WidthRequest = Device.RuntimePlatform == Device.UWP ? 400 : 100;

            layout.Children.Add(indicator);

            return layout;
        }
    }
}