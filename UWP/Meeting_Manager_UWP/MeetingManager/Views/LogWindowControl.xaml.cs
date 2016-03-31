using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeetingManager.Views
{
    public sealed partial class LogWindowControl : UserControl
    {
        private static int _logWindowId;

        public LogWindowControl()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            this.InitializeComponent();
        }

        private void ScrollToBottom()
        {
            var items = LogEntries.Items;

            if (items.Count > 0)
            {
                LogEntries.ScrollIntoView(items[items.Count - 1]);
            }
        }

        private void LogEntries_EnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollToBottom();
        }

        private async void NewWindowClick(object sender, RoutedEventArgs e)
        {
            _logWindowId = await NewWindow.Create(typeof(Views.LogWindowPage), "LOG WINDOW", _logWindowId);
        }
    }
}
