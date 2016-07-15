//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;

namespace MeetingManager.ViewModels
{
    class DialogViewModel : ViewModel, ITransientViewModel
    {
        public DialogViewModel()
        {
            UI.Subscribe<InitDialog>(OnInitialize);
        }

        protected virtual void OnInitialize(InitDialog parameter)
        {
            UI.Unsubscribe<InitDialog>(OnInitialize);
        }
    }
}
