//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;

namespace MeetingManager.ViewModels
{
    public class Loading : IDisposable
    {
        private readonly BaseViewModel _vm;

        public Loading(BaseViewModel vm)
        {
            _vm = vm;
            vm.IsLoading = true;
        }

        public void Dispose()
        {
            _vm.IsLoading = false;
        }
    }
}
