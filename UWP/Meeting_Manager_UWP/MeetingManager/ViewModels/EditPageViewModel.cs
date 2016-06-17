//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Commands;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace MeetingManager.ViewModels
{
    class EditPageViewModel : ViewModel
    {
        private Meeting _meeting;
        private ObservableCollection<Attendee> _attendees;
        private string _recurrenceDate;

        public EditPageViewModel()
        {
            GetEvent<UserSelectedEvent>().Subscribe(UserSelected);
            GetEvent<RoomSelectedEvent>().Subscribe(RoomSelected);
            GetEvent<MeetingTimeCandidateSelectedEvent>().Subscribe(MeetingTimeCandidateSelected);
            GetEvent<ContactSelectedEvent>().Subscribe(ContactSelected);
            GetEvent<MeetingRecurrenceUpdatedEvent>().Subscribe(RecurrenceUpdated);
        }

        public DelegateCommand SaveCommand => new DelegateCommand(SaveMeetingAsync);
        public DelegateCommand RecurrenceCommand => new DelegateCommand(SetRecurrence);
        public DelegateCommand AddUserCommand => new DelegateCommand(AddUser);
        public DelegateCommand AddContactCommand => new DelegateCommand(AddContact);
        public DelegateCommand FindRoomCommand => new DelegateCommand(FindRoom);
        public DelegateCommand GetSuggestedTimeCommand => new DelegateCommand(GetSuggestedTime, CanExecuteMeetingTimes);
        public DelegateCommand ASAPCommand => new DelegateCommand(ASAPMeeting, CanExecuteMeetingTimes);
        public DelegateCommand<Attendee> DeleteAttendeeCommand => new DelegateCommand<Attendee>(DeleteAttendee);
        public DelegateCommand ReplyAllCommand => new DelegateCommand(SendReplyAll);
        public DelegateCommand ForwardCommand => new DelegateCommand(SendForward);
        public DelegateCommand LateCommand => new DelegateCommand(SendLate);

        [RestorableState]
        public Meeting Meeting
        {
            get { return _meeting; }
            private set { SetProperty(ref _meeting, value); }
        }

        public string Title => GetString(IsNewMeeting ? "CreateMeetingTitle" : "UpdateMeetingTitle");

        public bool IsContentText => Meeting.IsContentText;

        public bool IsNewMeeting => string.IsNullOrEmpty(Meeting.Id);

        public DateTimeOffset StartDate
        {
            get
            {
                if (IsAllDay)
                {
                    return _meeting.Start.DateTime;
                }
                else
                {
                    return _meeting.Start.ToLocalTime();
                }
            }

            set
            {
                SetTime(_meeting.Start, value);

                if (_meeting.End.DateTime.CompareTo(_meeting.Start.DateTime) < 0)
                {
                    _meeting.End.DateTime = _meeting.Start.DateTime + TimeSpan.FromMinutes(30);
                    OnPropertyChanged(() => EndTime);
                    OnPropertyChanged(() => EndDate);
                }
            }
        }

        public DateTimeOffset EndDate
        {
            get
            {
                if (IsAllDay)
                {
                    return _meeting.End.DateTime;
                }
                else
                {
                    return _meeting.End.ToLocalTime();
                }
            }

            set
            {
                SetTime(_meeting.End, value);

                if (_meeting.Start.DateTime.CompareTo(_meeting.End.DateTime) > 0)
                {
                    _meeting.Start.DateTime = _meeting.End.DateTime - TimeSpan.FromMinutes(30);
                    OnPropertyChanged(() => StartTime);
                    OnPropertyChanged(() => StartDate);
                }
            }
        }

        private void SetTime(ZonedDateTime current, DateTimeOffset value)
        {
            if (IsAllDay)
            {
                current.DateTime = value.Date;
            }
            else
            {
                var local = _meeting.Start.ToLocalTime();
                var newTime = value.Date + local.TimeOfDay;

                current.DateTime = current.FromLocalTime(newTime);
            }
        }

        public string LocationName
        {
            get
            {
                return Meeting.Location.DisplayName;
            }
            set
            {
                Meeting.Location.DisplayName = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan StartTime
        {
            get { return GetTimeSpan(_meeting.Start); }
            set
            {
                SetTimeSpan(_meeting.Start, value);

                if (EndTime.CompareTo(value) < 0)
                {
                    EndTime = value + TimeSpan.FromMinutes(30);
                    OnPropertyChanged(() => EndTime);
                }
            }
        }

        public TimeSpan EndTime
        {
            get { return GetTimeSpan(_meeting.End); }
            set
            {
                SetTimeSpan(_meeting.End, value);

                if (StartTime.CompareTo(value) > 0)
                {
                    StartTime = value - TimeSpan.FromMinutes(30);
                    OnPropertyChanged(() => StartTime);
                }
            }
        }

        public ObservableCollection<Attendee> Attendees
        {
            get { return _attendees; }
            private set { SetProperty(ref _attendees, value); }
        }

        public string RecurrenceDate
        {
            get { return _recurrenceDate; }
            private set { SetProperty(ref _recurrenceDate, value); }
        }

        public string Description
        {
            get { return _meeting.Body.Content; }
            set { _meeting.Body.Content = value; }
        }

        public bool IsAllDay
        {
            get { return Meeting.IsAllDay; }
            set
            {
                if (value && !Meeting.IsAllDay)
                {
                    EnsureAllDay();
                }
                else if (!value && Meeting.IsAllDay)
                {
                    SetDefaultTimes(Meeting);
                }

                Meeting.IsAllDay = value;
                OnPropertyChanged();
                OnPropertyChanged(() => StartTime);
                OnPropertyChanged(() => EndTime);
            }
        }

        public bool IsSerial => _meeting.Recurrence != null; 

        public string SaveCaption => _meeting.Attendees.Any() ? GetString("SendCaption") : GetString("SaveCaption");

        public bool HasAttendees => Attendees.Any();

        private TimeSpan GetTimeSpan(ZonedDateTime dateTime)
        {
            if (IsAllDay)
            {
                return dateTime.DateTime.TimeOfDay;
            }
            else
            {
                var localDateTime = dateTime.ToLocalTime();
                return localDateTime.TimeOfDay;
            }
        }

        private static void SetTimeSpan(ZonedDateTime dateTime, TimeSpan timeSpan)
        {
            var localDateTime = dateTime.ToLocalTime();
            localDateTime = localDateTime.Date + timeSpan;

            dateTime.DateTime = dateTime.FromLocalTime(localDateTime);
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                {
                    _meeting = Deserialize<Meeting>(e.Parameter);
                }
                else
                {
                    _meeting = CreateNewMeeting();
                }
            }

            if (IsSerial)
            {
                RecurrenceDate = DateTimeUtils.BuildRecurrentDate(_meeting.Recurrence);
            }

            PopulateAttendees();
        }

        private void PopulateAttendees()
        {
            Attendees = new ObservableCollection<Attendee>();

            if (_meeting.Attendees != null)
            {
                foreach (var a in _meeting.Attendees)
                {
                    if (_meeting.Organizer != null)
                    {
                        a.IsOrganizer = a.EmailAddress.Address.EqualsCaseInsensitive(_meeting.Organizer.EmailAddress.Address);
                    }
                    Attendees.Add(a);
                }
            }

            OnPropertyChanged(() => SaveCaption);
            OnPropertyChanged(() => HasAttendees);
        }

        private Meeting CreateNewMeeting()
        {
            var meeting = new Meeting
            {
                Start = new ZonedDateTime(),
                End = new ZonedDateTime(),
                Body = new Body
                {
                    ContentType = "text"
                },
                Attendees = new List<Attendee>(),
                Location = new Location(),
                OriginalStartTimeZone = TimeZoneInfo.Local.Id,
                OriginalEndTimeZone = TimeZoneInfo.Local.Id
            };

            SetDefaultTimes(meeting);

            return meeting;
        }

        private DateTime GetDefaultStartTime()
        {
            // Use the start of the next hour as the default start time
            var dt = DateTime.Now + TimeSpan.FromMinutes(60);

            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 60) * 60, 0);
        }

        private void SetDefaultTimes(Meeting meeting)
        {
            meeting.Start = new ZonedDateTime
            {
                DateTime = GetDefaultStartTime(),
                TimeZone = TimeZoneInfo.Local.Id
            };

            meeting.End = new ZonedDateTime
            {
                DateTime = GetDefaultStartTime() + TimeSpan.FromMinutes(30),
                TimeZone = TimeZoneInfo.Local.Id
            };
        }

        private async void SaveMeetingAsync()
        {
            if (_meeting.IsAllDay)
            {
                EnsureAllDay();
            }

            Meeting newMeeting;
            using (new Loading(this))
            {
                newMeeting = await (string.IsNullOrEmpty(_meeting.Id) ?
                                        OfficeService.CreateEvent(_meeting) :
                                        OfficeService.UpdateEvent(_meeting));
            }

            if (newMeeting != null)
            {
                GetEvent<MeetingUpdatedEvent>().Publish(newMeeting);
                GoBack();
            }
        }

        private void EnsureAllDay()
        {
            var length = (_meeting.End.DateTime - _meeting.Start.DateTime).TotalDays;
            length = Math.Max(length, 1);
            // Set time to midnight (12:00 AM)
            _meeting.Start.DateTime = _meeting.Start.DateTime.Date;
            // Set the whole day duration
            _meeting.End.DateTime = _meeting.Start.DateTime + TimeSpan.FromDays((int) length);
            // It should be midnight in local time zone
            _meeting.Start.TimeZone = _meeting.End.TimeZone = TimeZoneInfo.Local.Id;
        }

        private void UserSelected(User user)
        {
            var attendee = new Attendee
            {
                EmailAddress = new EmailAddress
                {
                    Address = user.UserPrincipalName,
                    Name = user.DisplayName
                }
            };

            AddAttendee(attendee);
        }

        private void ContactSelected(Contact contact)
        {
            var attendee = new Attendee
            {
                EmailAddress = contact.EmailAddresses[0]
            };

            AddAttendee(attendee);
        }

        private void AddAttendee(Attendee attendee)
        {
            if (_meeting.Attendees.Find(x => x.EmailAddress.IsEqualTo(attendee.EmailAddress)) == null)
            {
                _meeting.Attendees.Add(attendee);
            }

            PopulateAttendees();
        }

        private void RoomSelected(User user)
        {
            LocationName = user.ToString();
        }

        private void DeleteAttendee(Attendee attendee)
        {
            int pos = _meeting.Attendees.IndexOf(x => x.EmailAddress.IsEqualTo(attendee.EmailAddress));
            _meeting.Attendees.RemoveAt(pos);

            PopulateAttendees();
        }

        private async void SetRecurrence()
        {
            var recurrence = Meeting.Recurrence ?? CreateDefaultRecurrence();

            if (!recurrence.Range.Type.EqualsCaseInsensitive(OData.EndDate))
            {
                recurrence.Range.EndDate = new DateTime(DateTime.Now.Year, 12, 31).ToString();
            }

            await NavigateTo("Recurrence", recurrence);
        }

        private Meeting.EventRecurrence CreateDefaultRecurrence()
        {
            return new Meeting.EventRecurrence
            {
                Pattern = new Meeting.Pattern()
                {
                    Type = OData.Daily,
                    Interval = 1,
                    DayOfMonth = 1,
                    Month = 1,
                    FirstDayOfWeek = OData.Sunday,
                    DaysOfWeek = new List<string>()
                },
                Range = new Meeting.Range
                {
                    Type = OData.NoEnd,
                    NumberOfOccurrences = 10,
                    StartDate = DateTime.Now.Date.ToString(),
                }
            };
        }

        private void RecurrenceUpdated(Meeting.EventRecurrence recurrence)
        {
            _meeting.Recurrence = recurrence;
            OnPropertyChanged(() => IsSerial);

            if (IsSerial)
            {
                RecurrenceDate = DateTimeUtils.BuildRecurrentDate(_meeting.Recurrence);
            }
        }

        private async void GetSuggestedTime()
        {
            await NavigateTo("TimeSlots", Meeting);
        }

        private async void ASAPMeeting()
        {
            var items = await GetAllTimeCandidates(_meeting);

            var slot = items.Select(x => TimeSlot.Parse(x))
                            .OrderBy(x => x.Start)
                            .FirstOrDefault(x => x.Start > DateTime.Now);

            if (slot != null)
            {
                SetTimeSlot(slot);
            }
        }

        private MeetingTimeCandidate SelectEarliestSlot(IEnumerable<MeetingTimeCandidate> items)
        {
            var ordered = items.OrderBy(x => DateTime.Parse(x.MeetingTimeSlot.Start.Time));

            return ordered.FirstOrDefault(x => DateTime.Parse(x.MeetingTimeSlot.Start.Time) > DateTime.Now);
        }

        private void MeetingTimeCandidateSelected(MeetingTimeCandidate meetingTimeCandidate)
        {
            if (meetingTimeCandidate != null)
            {
                var slot = TimeSlot.Parse(meetingTimeCandidate);
                SetTimeSlot(slot);
            }
        }

        private void SetTimeSlot(TimeSlot timeSlot)
        {
            StartTime = timeSlot.Start.TimeOfDay;
            EndTime = timeSlot.End.TimeOfDay;
        }

        private async void AddUser()
        {
            await NavigateToUsers(true);
        }

        private async void FindRoom()
        {
            await NavigateToUsers(false);
        }

        private async void AddContact()
        {
            await NavigateToContacts();
        }

        private async void SendReplyAll()
        {
            await NavigateToEmail(OData.ReplyAll);
        }

        private async void SendForward()
        {
            await NavigateToEmail(OData.Forward);
        }

        private async void SendLate()
        {
            await SendRunningLate(Meeting);
        }

        private async Task NavigateToEmail(string action, string comment = null)
        {
            await base.NavigateToEmail(Meeting, action, comment);
        }

        private bool CanExecuteMeetingTimes()
        {
            return !IsAllDay && App.Me.UseHttp;
        }
    }
}
