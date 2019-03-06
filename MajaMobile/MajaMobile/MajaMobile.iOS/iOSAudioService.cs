using AVFoundation;
using Foundation;
using MajaMobile.Interfaces;
using Speech;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(MajaMobile.iOS.iOSAudioService))]
namespace MajaMobile.iOS
{
    class iOSAudioService : IAudioService
    {
        private Timer _recognizerTimer;
        private string _recognizerText;
        private SFSpeechRecognizer _recognizer = new SFSpeechRecognizer(NSLocale.FromLocaleIdentifier("de-DE"));
        private SFSpeechRecognitionTask _task;
        private AVAudioEngine engine = new AVAudioEngine();

        public event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionPartialResult;
        public event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionResult;

        public iOSAudioService()
        {
            SFSpeechRecognizer.RequestAuthorization(SpeechRecognizerAuthChanged);
        }

        public void StartSpeechRecognition()
        {
            StopSpeechRecognizer(false);
            StopAudio();
            _recognizerText = "";
            AVAudioSession.SharedInstance().SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);

            var request = new SFSpeechAudioBufferRecognitionRequest();
            request.ShouldReportPartialResults = true;

            var inputNode = engine.InputNode;

            _task = _recognizer.GetRecognitionTask(request, (result, error) =>
            {
                if (result != null)
                {
                    _recognizerText = result.BestTranscription.FormattedString;
                    if (_recognizerTimer == null)
                    {
                        _recognizerTimer = new Timer(RecognizeTimerElapsed, this, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        _recognizerTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
                    }
                    if (result.Final)
                    {
                        SpeechRecognitionResult?.Invoke(this, new SpeechRecognitionEventArgs(_recognizerText));
                    }
                    else
                    {
                        SpeechRecognitionPartialResult?.Invoke(this, new SpeechRecognitionEventArgs(_recognizerText));
                    }
                }
                if (error != null || result?.Final == true)
                {
                    StopSpeechRecognizer(false);
                }
            });

            var recordingFormat = inputNode.GetBusOutputFormat(0);
            inputNode.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) =>
            {
                request.Append(buffer);
            });
            engine.Prepare();

            NSError engineError = null;
            if (engine.StartAndReturnError(out engineError))
            {

            }
        }

        private void RecognizeTimerElapsed(object state)
        {
            StopSpeechRecognizer(false);
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => SpeechRecognitionResult?.Invoke(this, new SpeechRecognitionEventArgs(_recognizerText)));//Necessary to run on UI thread
        }

        private void SpeechRecognizerAuthChanged(SFSpeechRecognizerAuthorizationStatus status)
        {

        }

        private async void StopSpeechRecognizer(bool waitForSpeech)
        {
            try
            {
                var task = _task;
                _task = null;
                if (task != null)
                {
                    if (waitForSpeech)
                    {
                        await Task.Delay(1000);
                    }
                    task.Cancel();
                    engine.Stop();
                    engine.InputNode.RemoveTapOnBus(0);
                }
            }
            catch (Exception e)
            {

            }
        }

        AVSpeechSynthesizer _speechSynthesizer;
        private AVSpeechSynthesisVoice _voice;

        public void PlayAudio(string text)
        {
            if (_speechSynthesizer == null)
            {
                _speechSynthesizer = new AVSpeechSynthesizer();
                _voice = AVSpeechSynthesisVoice.FromIdentifier("com.apple.ttsbundle.siri_female_de-DE_compact");
                if (_voice == null)
                    _voice = AVSpeechSynthesisVoice.FromLanguage("de-DE");
            }
            var speechUtterance = new AVSpeechUtterance(text)
            {
                Rate = AVSpeechUtterance.DefaultSpeechRate,
                Voice = _voice,
                Volume = 1.0f,
                PitchMultiplier = 1.0f
            };
            
            _speechSynthesizer.SpeakUtterance(speechUtterance);
        }

        public void StopAudio()
        {
            if (_speechSynthesizer != null && _speechSynthesizer.Speaking)
            {
                try
                {
                    _speechSynthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
                }
                catch (Exception) { }
            }
        }

        public void StopService()
        {
            StopAudio();
            StopSpeechRecognizer(true);
        }
    }
}