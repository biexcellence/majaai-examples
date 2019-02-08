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

        public double Duration { get; private set; } = 0.001;
        public double CurrentPosition => IsPlaying && _player != null ? _player.CurrentPosition : 0.0;

        public string CurrentPositionDisplay
        {
            get
            {
                if (IsPlaying && _player != null)
                    return TimeSpan.FromSeconds(_player.CurrentPosition).ToString(@"%m\:ss");
                return TimeSpan.FromSeconds(Duration).ToString(@"%m\:ss");
            }
        }

        private string _originalText;

        public MajaConversationMessageAudio(IMajaQueryAnswer queryAnswer) : base(queryAnswer)
        {
            AudioTappedCommand = new Command(PlayTapped);
            _originalText = Text;
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
                                if (_player.Duration > 0.0)
                                    Duration = _player.Duration;
                                OnPropertyChanged(nameof(Duration));
                                OnPropertyChanged(nameof(CurrentPositionDisplay));
                                _player.Play();
                                Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
                                {
                                    OnPropertyChanged(nameof(CurrentPosition));
                                    OnPropertyChanged(nameof(CurrentPositionDisplay));
                                    return _player.IsPlaying;
                                });
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
            OnPropertyChanged(nameof(CurrentPosition));
            OnPropertyChanged(nameof(CurrentPositionDisplay));
        }

        private void _player_PlaybackEnded(object sender, EventArgs e)
        {
            StopAudio();
        }
    }
}