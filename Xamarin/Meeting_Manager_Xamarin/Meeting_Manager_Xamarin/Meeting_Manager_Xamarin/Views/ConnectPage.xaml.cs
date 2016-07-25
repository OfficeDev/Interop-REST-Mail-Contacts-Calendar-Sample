//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Identity.Client;
using System;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.Views
{
    public partial class ConnectPage : ContentPage
    {
        public IPlatformParameters PlatformParameters { get; set; }

        public ConnectPage()
        {
            InitializeComponent();

            ServicePicker.Items.Add(ResMan.GetString("GraphHttpOption"));
            ServicePicker.Items.Add(ResMan.GetString("GraphSdkOption"));

            AuthPicker.Items.Add(ResMan.GetString("AuthHttpOption"));
            AuthPicker.Items.Add(ResMan.GetString("AuthMsalOption"));

            ServicePicker.SelectedIndex = 0;
            AuthPicker.SelectedIndex = 0;
        }

        public async void OnClicked(object sender, EventArgs args)
        {
            App.Me.CreateServices(PlatformParameters);

            if (!App.Me.UseMSAL)
            {
                await Navigation.PushAsync(new LoginPage());
            }
            else
            {
                await App.Me.InitializeApp();
            }
        }

        public void OnServiceSelectedIndexChanged(object sender, EventArgs args)
        {
            App.Me.UseHttp = ServicePicker.SelectedIndex == 0;
        }

        public void OnAuthSelectedIndexChanged(object sender, EventArgs args)
        {
            App.Me.UseMSAL = AuthPicker.SelectedIndex == 1;
        }
    }
}
