using System;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class CustomSlider : Slider
    {
        public static readonly BindableProperty CurrentStepValueProperty = BindableProperty.Create(nameof(StepValue), typeof(double), typeof(CustomSlider), 1.0, propertyChanged: StepValueChanged);

        public double StepValue
        {
            get { return (double)GetValue(CurrentStepValueProperty); }
            set
            {
                SetValue(CurrentStepValueProperty, value);
                SetNumDecimals();
            }
        }

        private static void StepValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var slider = (CustomSlider)bindable;
            if ((double)newValue <= 0.0)
                slider.StepValue = (double)oldValue;
        }

        private void SetNumDecimals()
        {
            _stepFactor = 0;
            if (StepValue < 1)
            {
                _numDecimals = GetNumDecimals(StepValue);
            }
        }

        private int GetNumDecimals(double d)
        {
            int numDecimals = -1;
            while (d > 0)
            {
                var i = (int)Math.Floor(d);
                if (_stepFactor == 0 && i > 0)
                {
                    _stepFactor = 1 / d;
                    return numDecimals + 1;
                }
                d = d - i;
                d = d * 10;
                numDecimals++;
            }
            return numDecimals;
        }

        private int _numDecimals = 0;
        private double _stepFactor = 0;

        public CustomSlider()
        {
            ValueChanged += OnSliderValueChanged;
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (_numDecimals > 1)
                return;
            if (StepValue >= 1)
            {
                var newStep = e.NewValue / StepValue;
                newStep = Math.Round(newStep, _numDecimals);
                Value = newStep * StepValue;
            }
            else if(StepValue == 0.1)
            {
                Value = Math.Round(e.NewValue, 1);
            }
            else
            {
                var newStep = e.NewValue * _stepFactor;
                newStep = Math.Round(newStep, _numDecimals);
                Value = newStep / _stepFactor;
            }
        }
    }
}