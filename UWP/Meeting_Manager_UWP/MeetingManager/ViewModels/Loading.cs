using System;

namespace MeetingManager.ViewModels
{
    public class Loading : IDisposable
    {
        private readonly ViewModel _vm;

        public Loading(ViewModel vm)
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
