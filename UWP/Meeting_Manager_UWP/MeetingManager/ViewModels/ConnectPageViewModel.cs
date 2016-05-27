//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Prism.Commands;

namespace MeetingManager.ViewModels
{
    class ConnectPageViewModel : ViewModel
    {
        public DelegateCommand ConnectCommand => new DelegateCommand(async() => await NavigateTo("Login"));
    }
}
