//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    public class CalendarPageViewModel : BaseViewModel
    {
        private DateTime _selectedDate;
        private ObservableCollection<Meeting> _meetings;
        private Meeting _selectedItem;

        public CalendarPageViewModel()
        {
            _selectedDate = DateTime.Now;

            Subscribe<Meeting>((sender, data) => RefreshMeetings());
        }

        public Command RefreshCommand => new Command(RefreshMeetings);
        public Command CreateCommand => new Command(CreateMeeting);
        public Command<Meeting> ViewCommand => new Command<Meeting>(ViewMeeting);
        public Command<Meeting> CancelCommand => new Command<Meeting>(CancelMeeting);
        public Command<Meeting> LateCommand => new Command<Meeting>(SendLate);
        public Command ListTappedCommand => new Command(ListTapped);

        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                SetProperty(ref _selectedDate, value);
                RefreshMeetings();
            }
        }

        public Meeting SelectedMeeting
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public ObservableCollection<Meeting> Meetings
        {
            get { return _meetings; }
            set { SetProperty(ref _meetings, value); }
        }

        public override async void OnAppearing(object data)
        {
            await GetEventsForSelectedDate();
        }

        private async void RefreshMeetings()
        {
            await GetEventsForSelectedDate();
        }

        private void ViewMeeting(Meeting meeting)
        {
            DoSeriesOrInstance(meeting, GetString("ViewMeetingCaption"), ViewMeetingSeries, ViewMeetingInstance);
        }

        private void CancelMeeting(Meeting meeting)
        {
            DoSeriesOrInstance(meeting, GetString("CancelMeetingCaption"), CancelMeetingSeries, CancelMeetingInstance);
        }

        private async void DoSeriesOrInstance(Meeting meeting, string title, Action<Meeting> seriesAction, Action<Meeting> instanceAction)
        {
            if (meeting.IsSerial)
            {
                var series = GetString("SeriesOption");
                var instance = GetString("InstanceOption");

                var action = await UI.DisplayActions(title, series, instance);

                if (action == series)
                {
                    seriesAction(meeting);
                }
                else if (action == instance)
                {
                    instanceAction(meeting);
                }
            }
            else
            {
                instanceAction(meeting);
            }
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

            return await UI.YesNoDialog(string.Format(format, meeting.Subject));
        }

        private async Task CancelMeeting(string id)
        {
            await GraphService.CancelEvent(id);
            await GetEventsForSelectedDate();
        }

        private async void CreateMeeting()
        {
            await UI.NavigateTo("Edit");
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

        private async void ListTapped()
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

        private async void SendLate(Meeting meeting)
        {
            await SendRunningLate(meeting);
        }

        private async Task<Meeting> GetEventById(string eventId)
        {
            using (new Loading(this))
            {
                return await GraphService.GetEvent(eventId);
            }
        }

        private async Task GetEventsForSelectedDate()
        {
            using (new Loading(this))
            {
                var events = await GraphService.GetCalendarEvents(
                            SelectedDate - TimeSpan.FromDays(1),
                            SelectedDate + TimeSpan.FromDays(2));

                var selectedDateEvents = events
                    .Where(x =>
                        x.IsAllDay ?
                        x.Start.DateTime.Date.CompareTo(SelectedDate.Date) <= 0 && x.End.DateTime.Date.CompareTo(SelectedDate.Date) > 0 :
                        x.Start.DateTime.ToLocalTime().Date.CompareTo(SelectedDate.Date) == 0)
                    .ToList();

                // We use Index property just for visualization
                int index = 0;
                selectedDateEvents.ForEach(ev => ev.Index = index++);

                Meetings = UI.UpdateObservableCollection<Meeting>(ref _meetings, selectedDateEvents);
                NotifyPropertyChanged(() => Meetings);
            }
        }
    }
}
