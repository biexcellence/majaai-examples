using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.GestureRecognizers
{
    public class PressedGestureRecognizer : Element, IGestureRecognizer
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(PressedGestureRecognizer), (object)null, BindingMode.OneWay);

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(PressedGestureRecognizer), (object)null, BindingMode.TwoWay);

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, (object)value); }
        }

        public object CommandParameter
        {
            get { return this.GetValue(CommandParameterProperty); }
            set { this.SetValue(CommandParameterProperty, value); }
        }
    }
    public class ReleasedGestureRecognizer : Element, IGestureRecognizer
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ReleasedGestureRecognizer), (object)null, BindingMode.OneWay);

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ReleasedGestureRecognizer), (object)null, BindingMode.TwoWay);

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, (object)value); }
        }

        public object CommandParameter
        {
            get { return this.GetValue(CommandParameterProperty); }
            set { this.SetValue(CommandParameterProperty, value); }
        }
    }
}