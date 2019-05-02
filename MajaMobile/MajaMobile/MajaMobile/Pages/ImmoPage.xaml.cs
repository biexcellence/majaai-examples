using MajaMobile.Messages;
using MajaMobile.ViewModels;

namespace MajaMobile.Pages
{
    public partial class ImmoPage : ContentPageBase
    {
        public ImmoPage(MajaConversationMessageImmo message)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ImmoPageViewModel(message);
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class ImmoPageViewModel : ViewModelBase
    {
        public MajaConversationMessageImmo Message { get; }

        public ImmoPageViewModel(MajaConversationMessageImmo message)
        {
            Message = message;
        }
    }
}