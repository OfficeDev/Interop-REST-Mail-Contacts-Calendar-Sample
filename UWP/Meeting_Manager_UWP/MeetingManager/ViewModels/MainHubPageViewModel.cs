using MeetingManager.Models;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager.ViewModels
{
    public class MainHubPageViewModel : ViewModel
    {
//        private readonly ICalendarControlViewModel _calendarControlViewModel;
//        private readonly IEditUserControlViewModel _editUserControlViewModel;

        public MainHubPageViewModel(/*ICalendarControlViewModel calendarControlViewModel/*,
            IEditControlViewModel editUserControlViewModel*/)
        {
//            _calendarControlViewModel = calendarControlViewModel;
//            _editUserControlViewModel = editUserControlViewModel;

//            GetEvent<CreateMeetingEvent>().Subscribe(OnCreateMeeting);
//            GetEvent<EditOpenEvent>().Subscribe(OnEditOpen);
//            GetEvent<EditCloseEvent>().Subscribe(OnEditClose);
        }

        public bool IsEditActive { get; private set; }
        public bool IsEmailActive { get; private set; }

        public bool IsDetailsActive
        {
            get
            {
                return !IsEditActive && !IsEmailActive;
            }
        }

        //public ICalendarControlViewModel CalendarControlViewModel
        //{
        //    get { return _calendarControlViewModel; }
        //}

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
//            CalendarControlViewModel.OnNavigatedTo(e, viewModelState);
        }
#if false
        private void OnCreateMeeting(object param)
        {
            IsEditActive = true;
            OnPropertyChanged(() => IsEditActive);

            _editUserControlViewModel.OnNavigatedTo(
                new NavigatedToEventArgs
                {
                    NavigationMode = Windows.UI.Xaml.Navigation.NavigationMode.New
                }
                , null);
        }
#endif
        private void OnEditOpen(Meeting meeting)
        {
            IsEditActive = true;
            OnPropertyChanged(() => IsEditActive);
            OnPropertyChanged(() => IsDetailsActive);
        }

        private void OnEditClose(object param)
        {
            IsEditActive = false;
            OnPropertyChanged(() => IsEditActive);
            OnPropertyChanged(() => IsDetailsActive);
        }
    }
}
