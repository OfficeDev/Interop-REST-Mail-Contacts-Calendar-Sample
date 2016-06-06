//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

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
