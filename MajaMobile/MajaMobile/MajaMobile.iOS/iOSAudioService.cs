using AVFoundation;
using Foundation;
using MajaMobile.Interfaces;
using Speech;
using System;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(MajaMobile.iOS.iOSAudioService))]
namespace MajaMobile.iOS
{
    class iOSAudioService : IAudioService
    {
        private string _recognizerText;
        private SFSpeechRecognizer _recognizer = new SFSpeechRecognizer(NSLocale.FromLocaleIdentifier("de-DE"));
        private SFSpeechRecognitionTask _task;
        private AVAudioEngine engine = new AVAudioEngine();

        public event EventHandler<EventArgs> StartedAudio;
        public event EventHandler<EventArgs> CompletedAudio;
        public event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionPartialResult;
        public event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionResult;

        public iOSAudioService()
        {
            SFSpeechRecognizer.RequestAuthorization(SpeechRecognizerAuthChanged);
        }

        public void StartSpeechRecognition()
        {

            StopRecognizer();
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
                    if (result.Final)
                    {
                        System.Diagnostics.Debug.WriteLine("RESULT FINAL: " + _recognizerText);
                        SpeechRecognitionResult?.Invoke(this, new SpeechRecognitionEventArgs(_recognizerText));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("RESULT PARTIAL: " + _recognizerText);
                        SpeechRecognitionPartialResult?.Invoke(this, new SpeechRecognitionEventArgs(_recognizerText));
                    }
                }
                if (error != null || result?.Final == true)
                {
                    //   StopRecognizer();
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

        private void SpeechRecognizerAuthChanged(SFSpeechRecognizerAuthorizationStatus status)
        {

        }

        private async void StopRecognizer()
        {
            var text = _recognizerText;
            var task = _task;
            _task = null;
            _recognizerText = "";
            if (!string.IsNullOrEmpty(text))
            {
                await Task.Delay(1000);
                text = _recognizerText;
            }
            System.Diagnostics.Debug.WriteLine("STOP RECOGNIZER: " + _recognizerText);
            try
            {
                if (task != null)
                {
                    task.Cancel();
                    engine.Stop();
                    engine.InputNode.RemoveTapOnBus(0);
                    _task = null;
                }
            }
            catch (Exception e)
            {

            }
            _recognizerText = "";
            if (!string.IsNullOrEmpty(text))
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => SpeechRecognitionResult?.Invoke(this, new SpeechRecognitionEventArgs(text)));//Necessary to run on UI thread
        }

        AVSpeechSynthesizer _speechSynthesizer;

        public void PlayAudio(string text)
        {
            if (_speechSynthesizer == null)
            {
                _speechSynthesizer = new AVSpeechSynthesizer();
                _speechSynthesizer.DidFinishSpeechUtterance += _speechSynthesizer_DidFinishSpeechUtterance;
                _speechSynthesizer.DidStartSpeechUtterance += _speechSynthesizer_DidStartSpeechUtterance;
            }
            var speechUtterance = new AVSpeechUtterance(text)
            {
                Rate = AVSpeechUtterance.DefaultSpeechRate,
                Voice = AVSpeechSynthesisVoice.FromLanguage("de-DE"),
                Volume = 1.0f,
                PitchMultiplier = 1.0f
            };

            _speechSynthesizer.SpeakUtterance(speechUtterance);
        }

        private void _speechSynthesizer_DidStartSpeechUtterance(object sender, AVSpeechSynthesizerUteranceEventArgs e)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => StartedAudio?.Invoke(this, EventArgs.Empty));
        }

        private void _speechSynthesizer_DidFinishSpeechUtterance(object sender, AVSpeechSynthesizerUteranceEventArgs e)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => CompletedAudio?.Invoke(this, EventArgs.Empty));
        }

        public void StopService()
        {
            if (_speechSynthesizer != null && _speechSynthesizer.Speaking)
            {
                try
                {
                    _speechSynthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
                }
                catch (Exception) { }
            }
            StopRecognizer();
        }
    }
}