using System;
using System.Collections.Generic;

namespace MajaMobile.Interfaces
{
    public interface IAudioService
    {
        void PlayAudio(string text);
        event EventHandler<EventArgs> StartedAudio;
        event EventHandler<EventArgs> CompletedAudio;
        event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionPartialResult;
        event EventHandler<SpeechRecognitionEventArgs> SpeechRecognitionResult;
        void StartSpeechRecognition();
        void StopService();
    }
    public class SpeechRecognitionEventArgs : EventArgs
    {
        public List<string> Results { get; }
        public SpeechRecognitionEventArgs(List<string> results)
        {
            Results = results;
        }
        public SpeechRecognitionEventArgs(string result)
        {
            Results = new List<string>() { result };
        }
    }
}