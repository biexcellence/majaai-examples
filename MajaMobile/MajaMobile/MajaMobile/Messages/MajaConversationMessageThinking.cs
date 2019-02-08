using System;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageThinking : MajaConversationMessage
    {
        private bool _isThinking;
        public MajaConversationMessageThinking() : base("")
        {

        }
        public void StartThinking()
        {
            _isThinking = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(350), TimerCallback);
        }

        private bool TimerCallback()
        {
            if (_isThinking)
            {
                Text = Text == "..." ? "." : Text + ".";
            }
            return _isThinking;
        }

        public void StopThinking()
        {
            _isThinking = false;
        }
    }
}