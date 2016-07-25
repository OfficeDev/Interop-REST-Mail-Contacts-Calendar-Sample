//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class CalendarPage : ContentPage
    {
        public CalendarPage()
        {
            InitializeComponent();
        }

        public async void OnTapped(object sender, EventArgs args)
        {
            await App.Me.PushAsync(new LogWindowPage());
        }
    }
}
