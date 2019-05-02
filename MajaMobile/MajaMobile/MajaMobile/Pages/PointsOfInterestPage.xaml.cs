using MajaMobile.Interfaces;
using MajaMobile.Messages;
using MajaMobile.ViewModels;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class PointsOfInterestPage : ContentPageBase
    {
        public PointsOfInterestPage(MajaConversationMessagePoi message)
        {
            InitializeComponent();
            BindingContext = ViewModel = new PointsOfInterestViewModel(message);
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class PointsOfInterestViewModel : ViewModelBase
    {
        public MajaConversationMessagePoi Message { get; }
        public int SpanCount
        {
            get
            {
                return (int)(DependencyService.Get<IDeviceInfo>().ScreenWidth / 170);
            }
        }

        public PointsOfInterestViewModel(MajaConversationMessagePoi message)
        {
            Message = message;
        }
    }
}