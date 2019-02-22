using Android;
using Android.Runtime;
using Android.Speech;
using Android.Speech.Tts;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using MajaMobile.Interfaces;
using Plugin.CurrentActivity;
using System;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(MajaMobile.Droid.AndroidAudioService))]
namespace MajaMobile.Droid
{
    class AndroidAudioService : UtteranceProgressListener, IAudioService, TextToSpeech.IOnInitListener
    {
        private SpeechRecognizer _speechRecognizer;

        public event EventHandler<EventArgs> StartedAudio;
        public event EventHandler<EventArgs> CompletedAudio;
        public event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionPartialResult;
        public event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionResult;

        TextToSpeech _speaker;
        string _textToSpeak;
        public void PlayAudio(string text)
        {
            _textToSpeak = text;
            if (_speaker == null)
            {
                _speaker = new TextToSpeech(Android.App.Application.Context, this);
                _speaker.SetSpeechRate(1.0f);
                _speaker.SetOnUtteranceProgressListener(this);
            }
            else
            {
                _speaker.Speak(_textToSpeak, QueueMode.Flush, null, "MajaUtteranceId");
            }

        }
        public override void OnDone(string utteranceId)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => CompletedAudio?.Invoke(this, EventArgs.Empty));
        }

        public override void OnError(string utteranceId)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => CompletedAudio?.Invoke(this, EventArgs.Empty));
        }

        public override void OnStart(string utteranceId)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => StartedAudio?.Invoke(this, EventArgs.Empty));
        }

        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status.Equals(OperationResult.Success))
            {
                _speaker.Speak(_textToSpeak, QueueMode.Flush, null, "MajaUtteranceId");
            }
        }

        public void StartSpeechRecognition()
        {
            try
            {
                if (ContextCompat.CheckSelfPermission(Android.App.Application.Context, Manifest.Permission.RecordAudio) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(CrossCurrentActivity.Current.Activity, new[] { Manifest.Permission.RecordAudio }, 0);
                }
                else
                {
                    if (_speechRecognizer == null)
                    {
                        _speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
                        _speechRecognizer.PartialResults += _speechRecognizer_PartialResults;
                        _speechRecognizer.Results += _speechRecognizer_Results;
                        _speechRecognizer.Error += _speechRecognizer_Error;
                    }
                    var intent = new Android.Content.Intent(RecognizerIntent.ActionRecognizeSpeech);
                    intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, "de");
                    intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Android.App.Application.Context.PackageName);
                    intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
                    intent.PutExtra(RecognizerIntent.ExtraMaxResults, 2);
                    intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                    intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                    intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);

                    _speechRecognizer.StartListening(intent);
                }
            }
            catch (Exception) { }
        }

        private void _speechRecognizer_Error(object sender, ErrorEventArgs e)
        {
            //Evaluate e.Error?
            //try
            //{
            //    SpeechRecognitionResult?.Invoke(this, new SpeechRecognitionEventArgs(""));
            //}
            //catch (Exception) { }
        }

        private void _speechRecognizer_Results(object sender, ResultsEventArgs e)
        {
            try
            {
                SpeechRecognitionResult?.Invoke(this, new SpeechRecognitionEventArgs(e.Results.GetStringArrayList(SpeechRecognizer.ResultsRecognition).ToList()));
            }
            catch (Exception) { }
        }

        private void _speechRecognizer_PartialResults(object sender, PartialResultsEventArgs e)
        {
            try
            {
                SpeechRecognitionPartialResult?.Invoke(this, new SpeechRecognitionEventArgs(e.PartialResults.GetStringArrayList(SpeechRecognizer.ResultsRecognition).First() + e.PartialResults.GetStringArrayList("android.speech.extra.UNSTABLE_TEXT").First()));
            }
            catch (Exception) { }
        }

        public void StopService()
        {
            if (_speaker != null && _speaker.IsSpeaking)
            {
                try
                {
                    _speaker.Stop();
                }
                catch (Exception) { }
            }
            if (_speechRecognizer != null)
            {
                try
                {
                    _speechRecognizer.StopListening();
                }
                catch (Exception) { }
            }
        }
    }
}