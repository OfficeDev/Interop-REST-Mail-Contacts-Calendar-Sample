using Prism.Windows.Navigation;
using System.Collections.Generic;

namespace MeetingManager.ViewModels
{
    public interface IChildViewModel
    {
        void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewState);
    }
}
