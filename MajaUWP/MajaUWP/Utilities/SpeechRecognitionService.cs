using BiExcellence.OpenBi.Api.Commands.MajaAi;
using MajaUWP.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        public event EventHandler<MajaQueryEventArgs> MajaQueryAnswersReceived;

        public CoreDispatcher Dispatcher => Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;

        /// <summary>
        /// This HResult represents the scenario where a user is prompted to allow in-app speech, but 
        /// declines. This should only happen on a Phone device, where speech is enabled for the entire device,
        /// not per-app.
        /// </summary>
        private static uint HResultPrivacyStatementDeclined = 0x80045509;

        public MajaConversation MajaConversation { get; private set; }

        public SpeechRecognitionService(MediaElement audioPlayer)
        {
            _audioPlayer = audioPlayer;
            MajaConversation = new MajaConversation(this);
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

        private async void ShowMessage(string message)
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

        private CancellationTokenSource _speechCancellationTokenSource;
        public async Task StopMaja()
        {
            try
            {
                if (_speechRecognizer.State == SpeechRecognizerState.Capturing)
                    await _speechRecognizer.StopRecognitionAsync();
                MajaConversation.MajaStatus = MajaListeningStatus.Idle;
            }
            catch { }

            var speechCancellationTokenSource = _speechCancellationTokenSource;
            if (speechCancellationTokenSource != null)
            {
                try
                {
                    speechCancellationTokenSource.Cancel();
                }
                catch { }
                speechCancellationTokenSource.Dispose();
            }
            _speechCancellationTokenSource = null;
            if (_audioPlayer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                _audioPlayer.Stop();
        }

        public async void StartSpeechRecognition()
        {
            await StopMaja();
            try
            {
                MajaConversation.MajaStatus = MajaListeningStatus.Listening;
                var result = await _speechRecognizer.RecognizeAsync();
                if (result.Confidence == SpeechRecognitionConfidence.Medium || result.Confidence == SpeechRecognitionConfidence.High)
                {
                    var status = await SendMajaQuery(result.Text);
                    if (status == MajaListeningStatus.Listening)
                    {
                        StartSpeechRecognition();
                        //_speechCancellationTokenSource = null;
                    }
                    else
                    {
                        MajaConversation.MajaStatus = status;
                    }
                }
                else if (!string.IsNullOrEmpty(result.Text))
                {
                    MajaConversation.Messages.Add(new MajaConversationMessage("Das habe ich akustisch nicht verstanden, bitte sprich direkt in das Mikrofon", MajaConversationSpeaker.Maja));
                    MajaConversation.MajaStatus = MajaListeningStatus.Idle;
                }
            }
            catch (InvalidOperationException invalidEx) when (invalidEx.HResult == -2146233079)
            {
                //TODO: is this correct?
                //_speechCancellationTokenSource = null;
                await InitializeRecognizer();
                StartSpeechRecognition();
            }
            catch (OperationCanceledException)
            {
                //_speechCancellationTokenSource = null;
                MajaConversation.MajaStatus = MajaListeningStatus.Idle;
            }
            catch (MajaServerException)
            {
                MajaConversation.MajaStatus = MajaListeningStatus.Idle;
            }
            catch (Exception ex)
            {
                //_speechCancellationTokenSource = null;
                MajaConversation.MajaStatus = MajaListeningStatus.Idle;

                if ((uint)ex.HResult == HResultPrivacyStatementDeclined)
                {
                    // Show a UI link to the privacy settings.
                    ShowMessage("Please restart the program, permission of microphone denied");
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping"));
                }
                else
                {
                    MajaConversation.Messages.Add(new MajaConversationMessage("Es kam beim Zugriff auf das Mikrofon zu einem Fehler. Bitte versuche es erneut.", MajaConversationSpeaker.Maja));
                }
            }
        }

        private void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text.ToLower();
            MajaConversation.SetUserText(hypothesis);
        }

        public async Task<MajaListeningStatus> SendMajaQuery(string value, string text = null, bool speak = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                MajaConversation.MajaStatus = MajaListeningStatus.Idle;
                return MajaConversation.MajaStatus;
            }
            var tokenSource = _speechCancellationTokenSource;
            if (tokenSource != null)
                tokenSource.Dispose();
            tokenSource = _speechCancellationTokenSource = new CancellationTokenSource();
            MajaListeningStatus status = MajaListeningStatus.Idle;
            IList<IMajaQueryAnswer> answers = null;
            try
            {
                answers = await MajaConversation.QueryMajaForAnswers(value, text, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MajaConversation.MajaQueryAnswer = null;
                MajaConversation.Messages.Clear();
                throw;
            }
            var tcs = new TaskCompletionSource<MajaListeningStatus>();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    status = await ReadMajaQueryAnswers(answers, speak);
                    tokenSource.Token.ThrowIfCancellationRequested();
                    tcs.TrySetResult(status != MajaListeningStatus.Unknown ? status : MajaListeningStatus.Idle);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                    tcs.TrySetResult(MajaListeningStatus.Idle);
                }
            });
            status = await tcs.Task;
            return status;
        }

        private async Task<MajaListeningStatus> ReadMajaQueryAnswers(IList<IMajaQueryAnswer> answers, bool speak = true)
        {
            if (answers.Count > 0)
            {
                var answer = answers.FirstOrDefault();
                if (answer == null)
                {
                    return MajaListeningStatus.Idle;
                }
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MajaQueryAnswersReceived?.Invoke(this, new MajaQueryEventArgs(answers));
                });
                switch (answer.ProposalType)
                {
                    case MajaQueryAnswerProposalType.AudioFile:
                        using (var client = new HttpClient())
                        using (var stream = await client.GetStreamAsync(answer.Action))
                        using (var memstream = new MemoryStream())
                        {
                            stream.CopyTo(memstream);
                            memstream.Position = 0;
                            await _audioPlayer.PlayStreamAsync(memstream.AsRandomAccessStream());
                        }
                        break;
                    case MajaQueryAnswerProposalType.VideoFile:
                    case MajaQueryAnswerProposalType.Location:
                    case MajaQueryAnswerProposalType.ImmoSuche:
                    case MajaQueryAnswerProposalType.Simple when (string.Equals(answer.Action, "weather", StringComparison.OrdinalIgnoreCase)):
                        break;
                    default:
                        if (speak && string.IsNullOrEmpty(answer.Url) && !string.IsNullOrEmpty(answer.Response))
                        {
                            await SpeakTextAsync(answer.Response);
                        }
                        break;
                }
                return answer.Completed ? MajaListeningStatus.Idle : MajaListeningStatus.Listening;
            }
            return MajaListeningStatus.Unknown;
        }

        public async Task SpeakTextAsync(string text)
        {
            var status = MajaConversation.MajaStatus;
            MajaConversation.MajaStatus = MajaListeningStatus.Speaking;
            try
            {
                using (var stream = await SynthesizeTextToSpeechAsync(text))
                {
                    await _audioPlayer.PlayStreamAsync(stream);
                }
            }
            catch (Exception) { }
        }

        async Task<IRandomAccessStream> SynthesizeTextToSpeechAsync(string text)
        {
            IRandomAccessStream stream = null;

            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                synthesizer.Voice = _majaVoice;
                stream = await synthesizer.SynthesizeTextToStreamAsync(text);
            }

            return (stream);
        }

        #endregion
    }
    public class MajaQueryEventArgs : EventArgs
    {
        public IList<IMajaQueryAnswer> MajaQueryAnswers { get; }
        public MajaQueryEventArgs(IList<IMajaQueryAnswer> majaQueryAnswers)
        {
            MajaQueryAnswers = majaQueryAnswers;
        }
    }
}