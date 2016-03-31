using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeetingManager.Views
{
    public sealed partial class TopBarControl : CommandBar
    {
        private int _logWindowId;

        public TopBarControl()
        {
            this.InitializeComponent();
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {

        }

        private async void LogWindowClick(object sender, RoutedEventArgs e)
        {
            if (_logWindowId == 0)
            {
                var newView = CoreApplication.CreateNewView();

                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = new Frame();
                    var n = frame.Navigate(typeof(Views.LogWindowPage));
                    Window.Current.Content = frame;
                    Window.Current.Activate();  // !!!

                    ApplicationView.GetForCurrentView().Title = "LOG WINDOW";

                    _logWindowId = ApplicationView.GetForCurrentView().Id;
                });
            }

            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(_logWindowId,
                ViewSizePreference.Default, ApplicationView.GetForCurrentView().Id, ViewSizePreference.Default);
        }
    }
}
