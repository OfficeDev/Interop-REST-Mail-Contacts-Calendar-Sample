using MeetingManager.Models;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MeetingManager.ViewModels
{
    public class CalendarPageViewModel : ViewModel
    {
        private ObservableCollection<Meeting> _meetings;
        private Meeting _selectedMeeting;
        private DateTimeOffset _selectedDate;

        public CalendarPageViewModel()
        {
            RefreshCommand = new DelegateCommand(RefreshMeetings);
            CreateCommand = new DelegateCommand(CreateMeeting);
            ViewInstanceCommand = new DelegateCommand<Meeting>(ViewMeetingInstance);
            CancelInstanceCommand = new DelegateCommand<Meeting>(CancelMeetingInstance);
            ViewSeriesCommand = new DelegateCommand<Meeting>(ViewMeetingSeries);
            CancelSeriesCommand = new DelegateCommand<Meeting>(CancelMeetingSeries);
            LateCommand = new DelegateCommand<Meeting>(SendLate);
            DatesChangedCommand = new DelegateCommand<CalendarViewSelectedDatesChangedEventArgs>(SelectedDatesChanged);
            DoubleTappedCommand = new DelegateCommand(DoubleTapped);
        }

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand CreateCommand { get; }
        public DelegateCommand<Meeting> CancelInstanceCommand { get; }
        public DelegateCommand<Meeting> ViewInstanceCommand { get; }
        public DelegateCommand<Meeting> CancelSeriesCommand { get; }
        public DelegateCommand<Meeting> ViewSeriesCommand { get; }
        public DelegateCommand<Meeting> LateCommand { get; }
        public DelegateCommand<CalendarViewSelectedDatesChangedEventArgs> DatesChangedCommand { get; }
        public DelegateCommand DoubleTappedCommand { get; }

        public ObservableCollection<Meeting> Meetings
        {
            get { return _meetings; }
            private set { SetProperty(ref _meetings, value); }
        }

        public Meeting SelectedMeeting
        {
            get { return _selectedMeeting; }
            set { SetProperty(ref _selectedMeeting, value); }
        }

        public DateTimeOffset SelectedDate
        {
            get { return _selectedDate; }
            private set { SetProperty(ref _selectedDate, value); }
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            if (e.NavigationMode == NavigationMode.New)
            {
                SelectedDate = DateTimeOffset.Now.Date;
            }

            await GetEventsForSelectedDate();
        }

        private async void RefreshMeetings()
        {
            await GetEventsForSelectedDate();
        }

        private async void CancelMeetingInstance(Meeting meeting)
        {
            if (await ConfirmCancellation(meeting))
            {
                await CancelMeeting(meeting.Id);
            }
        }

        private async void CancelMeetingSeries(Meeting meeting)
        {
            if (await ConfirmCancellation(meeting))
            {
                if (meeting.SeriesMasterId != null)
                {
                    meeting = await GetEventById(meeting.SeriesMasterId);
                }

                await CancelMeeting(meeting.Id);
            }
        }

        private async Task<bool> ConfirmCancellation(Meeting meeting)
        {
            string format;

            if (meeting.IsSerial)
            {
                format = GetString("CancelSeries");
            }
            else if (meeting.Type.EqualsCaseInsensitive(OData.Occurrence))
            {
                format = GetString("CancelOccurence");
            }
            else
            {
                format = GetString("CancelInstance");
            }

            return await YesNoDialog(string.Format(format, meeting.Subject));
        }

        private async Task CancelMeeting(string id)
        {
            await OfficeService.CancelEvent(id);
            await GetEventsForSelectedDate();
        }

        private async void CreateMeeting()
        {
            await NavigateTo("Edit");
        }

        private void ViewMeetingInstance(Meeting meeting)
        {
            NavigateToEditOrDetails(meeting);
        }

        private async void NavigateToEditOrDetails(Meeting meeting)
        {
            if (meeting != null)
            {
                await NavigateTo(meeting.IsOrganizer ? "Edit" : "Details", meeting);
            }
        }

        private async void ViewMeetingSeries(Meeting meeting)
        {
            if (meeting.SeriesMasterId != null)
            {
                meeting = await GetEventById(meeting.SeriesMasterId);
            }

            NavigateToEditOrDetails(meeting);
        }

        private async void DoubleTapped()
        {
            NavigateToEditOrDetails(await GetMeeting());
        }

        private async Task<Meeting> GetMeeting()
        {
            var meeting = SelectedMeeting;

            if (meeting != null && meeting.SeriesMasterId != null)
            {
                meeting = await GetEventById(meeting.SeriesMasterId);
            }

            return meeting;
        }

        private async void SelectedDatesChanged(CalendarViewSelectedDatesChangedEventArgs args)
        {
            var added = args.AddedDates;

            if (added.Count > 0)
            {
                SelectedDate = added[0];
                await GetEventsForSelectedDate();
            }
        }

        private async void SendLate(Meeting meeting)
        {
            await SendRunningLate(meeting);
        }

        private async Task<Meeting> GetEventById(string eventId)
        {
            return await OfficeService.GetEvent(eventId);
        }

        private async Task GetEventsForSelectedDate()
        {
            using (new Loading(this))
            {
                var events = await OfficeService.GetCalendarEvents(
                            SelectedDate - TimeSpan.FromDays(1),
                            SelectedDate + TimeSpan.FromDays(1));

                var selectedDateEvents = events.Where(x =>
                        x.IsAllDay ?
                        x.Start.DateTime.Date.CompareTo(SelectedDate.Date) == 0 :
                        x.Start.DateTime.ToLocalTime().Date.CompareTo(SelectedDate.Date) == 0);

                // We use Index property just for visualization
                int index = 0;
                selectedDateEvents.ForEach(ev => ev.Index = index++);

                Meetings = new ObservableCollection<Meeting>(selectedDateEvents);
            }
        }
    }
}
