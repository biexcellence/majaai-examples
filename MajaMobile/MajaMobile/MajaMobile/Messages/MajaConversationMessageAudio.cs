using BiExcellence.OpenBi.Api.Commands.MajaAi;
using Plugin.SimpleAudioPlayer;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Messages
{
    public class MajaConversationMessageAudio : MajaConversationMessage
    {
        public ICommand AudioTappedCommand { get; }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        private ISimpleAudioPlayer _player = CrossSimpleAudioPlayer.Current;
        
        private string _originalText;

        public MajaConversationMessageAudio(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            AudioTappedCommand = new Command(PlayTapped);
            _originalText = Text;
            PlayTapped();
        }

        protected override void MessageTapped()
        {
            PlayTapped();
        }

        private async void PlayTapped()
        {
            if (!IsPlaying && !string.IsNullOrEmpty(MajaQueryAnswer.Action))
            {
                if (_player != null)
                {
                    if (_player.IsPlaying)
                        _player.Stop();
                    _player.PlaybackEnded -= _player_PlaybackEnded;
                    _player.PlaybackEnded += _player_PlaybackEnded;
                    try
                    {
                        await Task.Run(async () =>
                        {
                            var client = new HttpClient();
                            using (var stream = await client.GetStreamAsync(MajaQueryAnswer.Action))
                            {
                                _player.Load(stream);
                                _player.Play();
                            }
                        });
                        IsPlaying = true;
                    }
                    catch
                    {
                        Text = _originalText + " (Fehler beim Abspielen der Datei)";
                        _player.PlaybackEnded -= _player_PlaybackEnded;
                    }
                }
            }
            else
            {
                StopAudio();
            }
        }

        public void StopAudio()
        {
            if (_player != null)
            {
                _player.PlaybackEnded -= _player_PlaybackEnded;
                if (_player.IsPlaying)
                {
                    _player.Stop();
                }
            }
            IsPlaying = false;
        }

        private void _player_PlaybackEnded(object sender, EventArgs e)
        {
            StopAudio();
        }
    }
}