using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MajaUWP.Extensions
{
    public static class ControlExtensions
    {
        public static async Task PlayStreamAsync(this MediaElement mediaElement, IRandomAccessStream stream)
        {
            // bool is irrelevant here, just using this to flag task completion.
            TaskCompletionSource<bool> taskCompleted = new TaskCompletionSource<bool>();

            // Note that the MediaElement needs to be in the UI tree for events
            // like MediaEnded to fire.
            RoutedEventHandler endOfPlayHandler = (s, e) =>
            {
                taskCompleted.SetResult(true);
            };
            RoutedEventHandler stateChangedHandler = (s, e) =>
            {
                if (mediaElement.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Stopped)
                {
                    taskCompleted.SetResult(true);
                }
            };
            ExceptionRoutedEventHandler failedHandler = (s, e) =>
            {
                taskCompleted.SetResult(true);
            };
            mediaElement.MediaEnded += endOfPlayHandler;
            mediaElement.CurrentStateChanged += stateChangedHandler;
            mediaElement.MediaFailed += failedHandler;

            mediaElement.SetSource(stream, string.Empty);
            mediaElement.Play();
            await taskCompleted.Task;
            mediaElement.MediaEnded -= endOfPlayHandler;
            mediaElement.CurrentStateChanged -= stateChangedHandler;
            mediaElement.MediaFailed -= failedHandler;
        }
    }
}