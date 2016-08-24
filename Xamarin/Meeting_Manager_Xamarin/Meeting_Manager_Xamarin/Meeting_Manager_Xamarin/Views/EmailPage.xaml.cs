//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.ViewModels;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class EmailPage : ContentPage
    {
        public EmailPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!App.Me.InTransient)
            {
                (BindingContext as BaseViewModel).NavigateFrom();
            }
        }
    }
}
