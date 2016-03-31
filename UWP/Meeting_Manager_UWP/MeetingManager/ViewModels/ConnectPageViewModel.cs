using Prism.Commands;

namespace MeetingManager.ViewModels
{
    class ConnectPageViewModel : ViewModel
    {
        public DelegateCommand ConnectCommand => new DelegateCommand(async() => await NavigateTo("Login"));
    }
}
