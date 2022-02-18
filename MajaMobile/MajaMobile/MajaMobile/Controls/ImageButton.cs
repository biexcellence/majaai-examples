using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace MajaMobile.Controls
{
    public class ImageButton : SfButton
    {
        public static readonly BindableProperty FontAwesomeIconProperty = BindableProperty.Create(nameof(FontAwesomeIcon), typeof(string), typeof(ImageButton), string.Empty);

        public string FontAwesomeIcon
        {
            get => (string)GetValue(FontAwesomeIconProperty);
            set => SetValue(FontAwesomeIconProperty, value);
        }

        public static readonly BindableProperty FontAwesomeIconColorProperty = BindableProperty.Create(nameof(FontAwesomeIconColor), typeof(Color), typeof(ImageButton), Color.Transparent);

        public Color FontAwesomeIconColor
        {
            get => (Color)GetValue(FontAwesomeIconColorProperty);
            set => SetValue(FontAwesomeIconColorProperty, value);
        }

        public static readonly BindableProperty FontTypeProperty = BindableProperty.Create(nameof(FontType), typeof(AwesomeFontFamily), typeof(ImageButton), AwesomeFontFamily.Regular);

        public AwesomeFontFamily FontType
        {
            get => (AwesomeFontFamily)GetValue(FontTypeProperty);
            set => SetValue(FontTypeProperty, value);
        }

        public string FontAwesomeFontFamily
        {
            get
            {
                if (FontType == AwesomeFontFamily.Light)
                {
                    if (Device.RuntimePlatform == Device.iOS)
                        return "FontAwesome5Pro-Light";
                    return "FontAwesomeLight.otf#Regular";
                }
                if (FontType == AwesomeFontFamily.Solid)
                {
                    if (Device.RuntimePlatform == Device.iOS)
                        return "FontAwesome5Pro-Solid";
                    return "FontAwesomeSolid.otf#Regular";
                }
                if (Device.RuntimePlatform == Device.iOS)
                    return "FontAwesome5Pro-Regular";
                return "FontAwesomeRegular.otf#Regular";
            }
        }

        public static readonly BindableProperty ContentTemplateProperty = BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(ImageButton), propertyChanged: ValidateTemplate);

        private static void ValidateTemplate(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImageButton button && newValue is DataTemplate template)
                button.TemplateSet(template);
        }

        private void TemplateSet(DataTemplate template)
        {
            Content = template.CreateContent() as View;
        }

        public AwesomeFontFamily ContentTemplate
        {
            get => (AwesomeFontFamily)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

    }

    public enum AwesomeFontFamily
    {
        Regular,
        Light,
        Solid,
    }
}