using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageThinking : MajaConversationMessage
    {
        public ICommand CancelTappedCommand { get; }
        public CancellationTokenSource CancellationTokenSource { get; }

        public MajaConversationMessageThinking() : base("")
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancelTappedCommand = new Command(() =>
            {
                try
                {
                    CancellationTokenSource.Cancel();
                }
                catch { }
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            CancellationTokenSource.Dispose();
        }
    }
}