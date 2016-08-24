//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingManager.ViewModels
{
    public class CalendarPageViewModel : BaseViewModel
    {
        private DateTimeOffset _selectedDate = DateTimeOffset.Now.Date;
        private ObservableCollection<Meeting> _meetings;

        public CalendarPageViewModel()
        {
            UI.Subscribe<Meeting>((meeting) => GetEventsForSelectedDate());
        }

        public Command RefreshCommand => new Command(GetEventsForSelectedDate);
        public Command CreateCommand => new Command(() => UI.NavigateTo("Edit"));
        public Command<Meeting> CancelInstanceCommand => new Command<Meeting>(CancelMeetingInstance);
        public Command<Meeting> ViewInstanceCommand => new Command<Meeting>((meeting) => NavigateToEditOrDetails(meeting));
        public Command<Meeting> CancelSeriesCommand => new Command<Meeting>(CancelMeetingSeries);
        public Command<Meeting> ViewSeriesCommand => new Command<Meeting>(ViewMeetingSeries);
        public Command<Meeting> LateCommand => new Command<Meeting>((meeting) => SendRunningLate(meeting));
        public Command SelectItemCommand => new Command(async () => NavigateToEditOrDetails(await GetMeeting()));

        public ObservableCollection<Meeting> Meetings
        {
            get { return _meetings; }
            private set { SetCollectionProperty(ref _meetings, value); }
        }

        public DateTimeOffset SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                SetProperty(ref _selectedDate, value);
                GetEventsForSelectedDate();
            }
        }

        public Meeting SelectedMeeting { get; set; }

        protected override void OnNavigatedTo(object parameter)
        {
            GetEventsForSelectedDate();
        }

        private async void CancelMeetingInstance(Meeting meeting)
        {
            if (await ConfirmCancellation(meeting, isInstance: true))
            {
                CancelMeeting(meeting.Id);
            }
        }

        private async void CancelMeetingSeries(Meeting meeting)
        {
            if (await ConfirmCancellation(meeting, isInstance: false))
            {
                if (meeting.SeriesMasterId != null)
                {
                    meeting = await GetEventById(meeting.SeriesMasterId);
                }

                CancelMeeting(meeting.Id);
            }
        }

        private async Task<bool> ConfirmCancellation(Meeting meeting, bool isInstance)
        {
            string format;

            if (meeting.IsSerial)
            {
                format = GetString(isInstance ? "CancelOccurence" : "CancelSeries");
            }
            else
            {
                format = GetString("CancelInstance");
            }

            return await UI.YesNoDialog(string.Format(format, meeting.Subject));
        }

        private async void CancelMeeting(string id)
        {
            await GraphService.CancelEvent(id);

            int pos = Meetings.IndexOf(x => x.Id == id);
            Meetings.RemoveAt(pos);

            // Make sure we have correct indexes
            GetEventsForSelectedDate(); 
        }

        private void NavigateToEditOrDetails(Meeting meeting)
        {
            if (meeting != null)
            {
                UI.NavigateTo(meeting.IsOrganizer ? "Edit" : "Details", meeting);
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

        private async Task<Meeting> GetMeeting()
        {
            var meeting = SelectedMeeting;

            if (meeting?.SeriesMasterId != null)
            {
                meeting = await GetEventById(meeting.SeriesMasterId);
            }

            return meeting;
        }

        private async Task<Meeting> GetEventById(string eventId)
        {
            using (new Loading(this))
            {
                return await GraphService.GetEvent(eventId);
            }
        }

        private async void GetEventsForSelectedDate()
        {
            IEnumerable<Meeting> events;

            using (new Loading(this))
            {
                events = await GraphService.GetCalendarEvents(
                            SelectedDate - TimeSpan.FromDays(1),
                            SelectedDate + TimeSpan.FromDays(2));
            }

            var selectedDateEvents = events
                .Where(x =>
                    x.IsAllDay ?
                    x.Start.DateTime.Date.CompareTo(SelectedDate.Date) <= 0 && x.End.DateTime.Date.CompareTo(SelectedDate.Date) > 0 :
                    x.Start.DateTime.ToLocalTime().Date.CompareTo(SelectedDate.Date) == 0)
                .ToList();

            // We use Index property just for visualization
            int index = 0;
            selectedDateEvents.ForEach(ev => ev.Index = index++);

            Meetings = new ObservableCollection<Meeting>(selectedDateEvents);
        }
    }
}
