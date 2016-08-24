//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;

namespace MeetingManager.ViewModels
{
    class ConnectPageViewModel : BaseViewModel
    {
        public bool UseHttp
        {
            get { return App.Me.UseHttp; }
            set { App.Me.UseHttp = value;}
        }

        public bool UseMSAL
        {
            get { return App.Me.UseMSAL; }
            set { App.Me.UseMSAL = value; }
        }

        public ConnectPageViewModel()
        {
            UseHttp = true;
            UseMSAL = false;
        }

        public Command ConnectCommand => new Command(Connect);

        private void Connect()
        {
            if (!App.Me.UseMSAL)
            {
                UI.NavigateTo("Login");
            }
            else
            {
                App.Me.InitializeApp(null);
            }
        }
    }
}
