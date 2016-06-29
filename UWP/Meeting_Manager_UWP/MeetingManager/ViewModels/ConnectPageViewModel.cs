//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Prism.Commands;
using Windows.UI.Xaml;

namespace MeetingManager.ViewModels
{
    class ConnectPageViewModel : ViewModel
    {
        public bool UseHttp
        {
            get
            {
                return (Application.Current as App).UseHttp;
            }

            set
            {
                (Application.Current as App).UseHttp = value;
            }
        }

        public bool UseSDK
        {
            get
            {
                return !UseHttp;
            }

            set
            {
                (Application.Current as App).UseHttp = !value;
            }
        }

        public ConnectPageViewModel()
        {
            UseHttp = true;
        }

        public DelegateCommand ConnectCommand => new DelegateCommand(async() => await UI.NavigateTo("Login"));
    }
}
