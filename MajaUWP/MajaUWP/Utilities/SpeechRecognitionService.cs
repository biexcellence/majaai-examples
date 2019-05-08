using MajaUWP.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace MajaUWP.Utilities
{
    public class SpeechRecognitionService : IDisposable
    {

        #region SpeechRecognizer
        private static VoiceInformation _majaVoice;
        private SpeechRecognizer _speechRecognizer;
        private MediaElement _audioPlayer;
        public event EventHandler<SpeechRecognitionHypothesisGeneratedEventArgs> HypothesisGenerated;

        public CoreDispatcher Dispatcher => Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;

        public SpeechRecognitionService(MediaElement audioPlayer)
        {
            _audioPlayer = audioPlayer;
            if (_majaVoice == null)
                _majaVoice = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Id == @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_deDE_KatjaM");
            InitializeRecognizer();
        }

        public void SetAudioPlayer(MediaElement audioPlayer)
        {
            _audioPlayer = audioPlayer;
        }

        public void Dispose()
        {
            _audioPlayer = null;
        }

        //TODO:move somewhere else
        public async void ShowMessage(string message)
        {
            if (Dispatcher.HasThreadAccess)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                await messageDialog.ShowAsync();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Exception");
                    await messageDialog.ShowAsync();
                });
            }
        }

        public async Task InitializeRecognizer()
        {
            if (_speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                DisposeSpeechRecognizer();
            }

            _speechRecognizer = new SpeechRecognizer(new Language("de-DE"));
            _speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

            //_speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(10);
            //_speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(10);
            //_speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(10);

            // Apply the dictation topic constraint to optimize for dictated freeform speech.
            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "maja");
            _speechRecognizer.Constraints.Add(dictationConstraint);
            SpeechRecognitionCompilationResult result = await _speechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                ShowMessage("Grammar Compilation Failed: ");
            }

            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
            // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
            // allows us to provide incremental feedback based on what the user's currently saying.

        }
        public void DisposeSpeechRecognizer()
        {
            if (_speechRecognizer != null)
            {
                _speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                _speechRecognizer.Dispose();
                _speechRecognizer = null;
            }
        }

        public async Task Stop()
        {
            try
            {
                if (_speechRecognizer.State == SpeechRecognizerState.Capturing || _speechRecognizer.State == SpeechRecognizerState.SpeechDetected)
                    await _speechRecognizer.StopRecognitionAsync();
            }
            catch { }
            if (_audioPlayer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                _audioPlayer.Stop();
        }

        public async Task<SpeechRecognitionResult> RecognizeAsync()
        {
            await Stop();
            try
            {
                return await _speechRecognizer.RecognizeAsync();
            }
            catch (InvalidOperationException invalidEx) when (invalidEx.HResult == -2146233079)
            {
                //TODO: is this correct?
                //_speechCancellationTokenSource = null;
                await InitializeRecognizer();
                return await RecognizeAsync();
            }
        }

        private void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            HypothesisGenerated?.Invoke(this, args);
        }

        //public async Task<MajaListeningStatus> SendMajaQuery(string value, string text = null, bool speak = true)
        //{
        //    if (string.IsNullOrWhiteSpace(value))
        //    {
        //        MajaConversation.MajaStatus = MajaListeningStatus.Idle;
        //        return MajaConversation.MajaStatus;
        //    }
        //    var tokenSource = _speechCancellationTokenSource;
        //    if (tokenSource != null)
        //        tokenSource.Dispose();
        //    tokenSource = _speechCancellationTokenSource = new CancellationTokenSource();
        //    MajaListeningStatus status = MajaListeningStatus.Idle;
        //    IList<IMajaQueryAnswer> answers = null;
        //    try
        //    {
        //        answers = await MajaConversation.QueryMajaForAnswers(value, text, tokenSource.Token);
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        MajaConversation.MajaQueryAnswer = null;
        //        MajaConversation.Messages.Clear();
        //        throw;
        //    }
        //    var tcs = new TaskCompletionSource<MajaListeningStatus>();
        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        //    {
        //        try
        //        {
        //            status = await ReadMajaQueryAnswers(answers, speak);
        //            tokenSource.Token.ThrowIfCancellationRequested();
        //            tcs.TrySetResult(status != MajaListeningStatus.Unknown ? status : MajaListeningStatus.Idle);
        //        }
        //        catch (Exception e)
        //        {
        //            tcs.TrySetException(e);
        //            tcs.TrySetResult(MajaListeningStatus.Idle);
        //        }
        //    });
        //    status = await tcs.Task;
        //    return status;
        //}

        public async Task PlayAudio(string uri)
        {
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(uri))
            using (var memstream = new MemoryStream())
            {
                stream.CopyTo(memstream);
                memstream.Position = 0;
                await _audioPlayer.PlayStreamAsync(memstream.AsRandomAccessStream());
            }
        }

        public async Task SpeakTextAsync(string text)
        {
            try
            {
                using (var stream = await SynthesizeTextToSpeechAsync(text))
                {
                    await _audioPlayer.PlayStreamAsync(stream);
                }
            }
            catch (Exception) { }
        }

        private async Task<IRandomAccessStream> SynthesizeTextToSpeechAsync(string text)
        {
            IRandomAccessStream stream = null;

            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                synthesizer.Voice = _majaVoice;
                stream = await synthesizer.SynthesizeTextToStreamAsync(text);
            }

            return stream;
        }

        #endregion
    }
}