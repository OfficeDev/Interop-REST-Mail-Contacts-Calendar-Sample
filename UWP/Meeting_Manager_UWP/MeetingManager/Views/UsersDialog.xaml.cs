using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MeetingManager.Views
{
    public sealed partial class UsersDialog : ContentDialog
    {
        public UsersDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}
