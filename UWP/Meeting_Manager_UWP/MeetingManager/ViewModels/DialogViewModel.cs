//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;

namespace MeetingManager.ViewModels
{
    abstract class DialogViewModel : BaseViewModel, ITransientViewModel
    {
        public DialogViewModel()
        {
            UI.Subscribe<InitDialog>(OnInitialize);
        }

        private void OnInitialize(InitDialog parameter)
        {
            UI.Unsubscribe<InitDialog>(OnInitialize);

            OnNavigatedTo(parameter.Payload);
        }

        protected override void OnNavigatedTo(object data)
        {
        }

        protected override void GoBack()
        {
            UI.CloseDialog();
        }
    }
}
