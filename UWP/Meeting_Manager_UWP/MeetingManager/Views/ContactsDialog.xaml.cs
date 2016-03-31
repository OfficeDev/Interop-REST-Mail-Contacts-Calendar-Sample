using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MeetingManager.Views
{
    public sealed partial class ContactsDialog : ContentDialog
    {
        public ContactsDialog()
        {
            this.InitializeComponent();
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}
