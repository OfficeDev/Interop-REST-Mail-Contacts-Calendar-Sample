using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MeetingManager.Views
{
    public sealed partial class TimeSlotsDialog : ContentDialog
    {
        public TimeSlotsDialog()
        {
            this.InitializeComponent();
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}
