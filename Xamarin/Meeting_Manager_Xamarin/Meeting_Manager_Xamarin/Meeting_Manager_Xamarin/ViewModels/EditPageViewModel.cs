//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Meeting_Manager_Xamarin.ViewModels
{
    class EditPageViewModel : BaseViewModel
    {
        private Meeting _meeting;
        private string _recurrenceDate;
        private List<FileAttachment> _attachments;

        public EditPageViewModel()
        {
            Subscribe<User>(UserSelected);
            Subscribe<Room>(RoomSelected);
            Subscribe<Contact>(ContactSelected);
            Subscribe<Meeting.EventRecurrence>(RecurrenceUpdated);
            Subscribe<DriveItem>(AttachmentSelected);
            Subscribe<FileAttachment>(AttachmentDeleted);
            Subscribe<MeetingTimeCandidate>(TimeSlotSelected);
        }

        public Command SaveCommand => new Command(SaveMeetingAsync);
        public Command AttachCommand => new Command(SelectFile);
        public Command RecurrenceCommand => new Command(SetRecurrence);
        public Command AddAttendeeCommand => new Command(AddAttendee);
        public Command FindRoomCommand => new Command(FindRoom);
        public Command ShowAttachmentsCommand => new Command(ShowAttachments);
        public Command TimeSlotsCommand => new Command(GetSuggestedTime, CanExecuteMeetingTimes);
        public Command<Attendee> DeleteAttendeeCommand => new Command<Attendee>(DeleteAttendee);
        public Command EmailCommand => new Command(SendEmail);

        public Meeting Meeting
        {
            get { return _meeting ?? CreateNewMeeting(); }
            private set { SetProperty(ref _meeting, value); }
        }

        public string Title => GetString(IsNewMeeting ? "CreateMeetingTitle" : "UpdateMeetingTitle");

        public bool IsContentText => Meeting.IsContentText;

        public bool HasAttachments => _attachments != null && _attachments.Any();

        public bool IsNewMeeting => string.IsNullOrEmpty(Meeting.Id);

        public DateTime StartDate
        {
            get
            {
                if (IsAllDay)
                {
                    return GetUtcDate(Meeting.Start.DateTime);
                }
                else
                {
                    return Meeting.Start.ToLocalTime();
                }
            }

            set
            {
                SetTime(Meeting.Start, value);

                if (Meeting.End.DateTime.CompareTo(Meeting.Start.DateTime) < 0)
                {
                    Meeting.End.DateTime = Meeting.Start.DateTime + TimeSpan.FromMinutes(30);
                    OnPropertyChanged(() => EndTime);
                    OnPropertyChanged(() => EndDate);
                }
            }
        }

        public DateTime EndDate
        {
            get
            {
                if (IsAllDay)
                {
                    return GetUtcDate(Meeting.End.DateTime);
                }
                else
                {
                    return Meeting.End.ToLocalTime();
                }
            }

            set
            {
                SetTime(Meeting.End, value);

                if (Meeting.Start.DateTime.CompareTo(Meeting.End.DateTime) > 0)
                {
                    Meeting.Start.DateTime = Meeting.End.DateTime - TimeSpan.FromMinutes(30);
                    OnPropertyChanged(() => StartTime);
                    OnPropertyChanged(() => StartDate);
                }
            }
        }

        private DateTime GetUtcDate(DateTime dateTime)
        {
            // Default DatePicker converter seems to convert from UTC to Local,
            // so we return fake Local date
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        }

        private void SetTime(ZonedDateTime current, DateTimeOffset value)
        {
            if (IsAllDay)
            {
                current.DateTime = value.Date;
            }
            else
            {
                var local = Meeting.Start.ToLocalTime();
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
            get
            {
                return GetTimeSpan(Meeting.Start);
            }

            set
            {
                if (!IsAllDay)
                {
                    SetTimeSpan(Meeting.Start, value);

                    if (EndTime.CompareTo(value) < 0)
                    {
                        EndTime = value + TimeSpan.FromMinutes(30);
                        OnPropertyChanged(() => EndTime);
                    }
                }
            }
        }

        public TimeSpan EndTime
        {
            get { return GetTimeSpan(Meeting.End); }
            set
            {
                if (!IsAllDay)
                {
                    SetTimeSpan(Meeting.End, value);

                    if (StartTime.CompareTo(value) > 0)
                    {
                        StartTime = value - TimeSpan.FromMinutes(30);
                        OnPropertyChanged(() => StartTime);
                    }
                }
            }
        }

        public ObservableCollection<Attendee> Attendees { get; set; }

        public string RecurrenceDate
        {
            get { return _recurrenceDate; }
            private set { SetProperty(ref _recurrenceDate, value); }
        }

        public string Description
        {
            get { return Meeting.Body.Content; }
            set { Meeting.Body.Content = value; }
        }

        public HtmlWebViewSource HtmlSource
        {
            get
            {
                return new HtmlWebViewSource()
                {
                    Html = Meeting.Body.Content ?? String.Empty
                };
            }
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
                OnPropertyChanged();    // trigger IsEnabled
                OnPropertyChanged(() => EndDate);
                OnPropertyChanged(() => StartTime);
                OnPropertyChanged(() => EndTime);
            }
        }

        public bool IsSerial => Meeting.Recurrence != null;

        public string SaveCaption => GetString(HasAttendees ? "SendCaption" : "SaveCaption");

        public bool HasAttendees => Meeting.Attendees.Any();

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

            dateTime.DateTime = TimeZoneInfo.ConvertTime(localDateTime, TimeZoneInfo.Utc);
        }

        public override async void OnAppearing(object data)
        {
            if (data != null)
            {
                _meeting = JSON.Deserialize<Meeting>(data);

                using (new Loading(this))
                {
                    _attachments = (await GraphService.GetEventAttachments(_meeting.Id, 0, 100))
                                    .ToList();
                    OnPropertyChanged(() => HasAttachments);
                }
            }
            else
            {
                _meeting = CreateNewMeeting();
                _attachments = new List<FileAttachment>();
            }

            if (IsSerial)
            {
                RecurrenceDate = DateTimeUtils.BuildRecurrentDate(_meeting.Recurrence);
            }

            PopulateAttendees();
        }

        private void PopulateAttendees()
        {
            Attendees = new ObservableCollection<Attendee>(Meeting.Attendees);
            OnPropertyChanged(() => Attendees);

            if (Meeting.Organizer != null)
            {
                foreach (var a in Meeting.Attendees)
                {
                    a.IsOrganizer = a.EmailAddress.Address.EqualsCaseInsensitive(Meeting.Organizer.EmailAddress.Address);
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
            };

            SetDefaultTimes(meeting);

            return meeting;
        }

        private DateTime GetDefaultStartTime()
        {
            // Use the start of the next hour as the default start time
            var dt = DateTime.UtcNow + TimeSpan.FromMinutes(60);

            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 60) * 60, 0, DateTimeKind.Utc);
        }

        private void SetDefaultTimes(Meeting meeting)
        {
            meeting.Start = new ZonedDateTime
            {
                DateTime = GetDefaultStartTime(),
                TimeZone = "UTC"
            };

            meeting.End = new ZonedDateTime
            {
                DateTime = GetDefaultStartTime() + TimeSpan.FromMinutes(30),
                TimeZone = "UTC"
            };
        }

        private async void SaveMeetingAsync()
        {
            if (Meeting.IsAllDay)
            {
                EnsureAllDay();
            }

            Meeting.HasAttachments = _attachments.Any();

            Meeting newMeeting = null;
            using (new Loading(this))
            {
                newMeeting = await (string.IsNullOrEmpty(Meeting.Id) ?
                                        GraphService.CreateEvent(Meeting) :
                                        GraphService.UpdateEvent(Meeting));
            }

            if (newMeeting != null)
            {
                foreach (var att in _attachments.Where(x => string.IsNullOrEmpty(x.Id)))
                {
                    using (new Loading(this))
                    {
                        await GraphService.AddEventAttachment(newMeeting.Id, att);
                    }
                }

                Publish<Meeting>(newMeeting);
                await UI.GoBack();
            }
        }

        private void EnsureAllDay()
        {
            var length = (Meeting.End.DateTime - Meeting.Start.DateTime).TotalDays;
            length = Math.Max(length, 1);
            // Set time to midnight (12:00 AM)
            Meeting.Start.DateTime = Meeting.Start.DateTime.Date;
            // Set the whole day duration
            Meeting.End.DateTime = Meeting.Start.DateTime + TimeSpan.FromDays((int)length);
        }

        private void UserSelected(object sender, User user)
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

        private void ContactSelected(object sender, Contact contact)
        {
            var attendee = new Attendee
            {
                EmailAddress = contact.EmailAddresses[0]
            };

            AddAttendee(attendee);
        }

        private void AttachmentDeleted(object sender, FileAttachment item)
        {
            var removed = _attachments.FirstOrDefault(x => x.Name == item.Name);
            _attachments.Remove(removed);

            OnPropertyChanged(() => HasAttachments);
        }

        private async void AttachmentSelected(object sender, DriveItem driveItem)
        {
            using (new Loading(this))
            {
                var data = await GraphService.GetDriveItemContent(driveItem.Id);

                if (data != null)
                {
                    _attachments.Add(new FileAttachment
                    {
                        Name = driveItem.Name,
                        ContentBytes = data,
                    });

                    OnPropertyChanged(() => HasAttachments);
                }
            }
        }

        private void AddAttendee(Attendee attendee)
        {
            if (Meeting.Attendees.Find(x => x.EmailAddress.IsEqualTo(attendee.EmailAddress)) == null)
            {
                Meeting.Attendees.Add(attendee);
            }

            PopulateAttendees();
        }

        private void RoomSelected(object sender, User user)
        {
            LocationName = user.ToString();
        }

        private void DeleteAttendee(Attendee attendee)
        {
            if (attendee.IsOrganizer) return;

            int pos = Meeting.Attendees.IndexOf(x => x.EmailAddress.IsEqualTo(attendee.EmailAddress));
            Meeting.Attendees.RemoveAt(pos);

            PopulateAttendees();
        }

        private async void SetRecurrence()
        {
            var recurrence = Meeting.Recurrence ?? CreateDefaultRecurrence();

            if (!recurrence.Range.Type.EqualsCaseInsensitive(OData.EndDate))
            {
                recurrence.Range.EndDate = new DateTime(DateTime.Now.Year, 12, 31).ToString();
            }

            await UI.NavigateTo("Recurrence", recurrence);
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

        private void RecurrenceUpdated(object sender, Meeting.EventRecurrence recurrence)
        {
            Meeting.Recurrence = recurrence;
            OnPropertyChanged(() => IsSerial);

            if (IsSerial)
            {
                RecurrenceDate = DateTimeUtils.BuildRecurrentDate(Meeting.Recurrence);
            }
        }

        private async void GetSuggestedTime()
        {
            await UI.DisplayAndExecuteAction(GetString("TimeSlotsCaption"), new Dictionary<string, Action>
            {
                [GetString("SelectSlotOption")] = NavigateToTimeSlots,
                [GetString("ASAPOption")] = ASAPMeeting,
            });
        }

        private async void NavigateToTimeSlots()
        {
            await NavigateTo("TimeSlots", Meeting);
        }

        private async void ASAPMeeting()
        {
            var items = await GetAllTimeCandidates(_meeting);

            var slot = items.Select(x => TimeSlot.Parse(x))
                            .OrderBy(x => x.Start)
                            .FirstOrDefault(x => _meeting.Start.DateTime.Date + x.Start > DateTime.Now);

            if (slot != null)
            {
                SetTimeSlot(slot);
            }
        }

        private void TimeSlotSelected(object sender, MeetingTimeCandidate meetingTimeCandidate)
        {
            if (meetingTimeCandidate != null)
            {
                var slot = TimeSlot.Parse(meetingTimeCandidate);
                SetTimeSlot(slot);
            }
        }

        private void SetTimeSlot(TimeSlot timeSlot)
        {
            StartTime = timeSlot.Start;
            EndTime = timeSlot.End;

            OnPropertyChanged(() => StartTime);
            OnPropertyChanged(() => EndTime);
        }

        private void AddAttendee()
        {
            BaseViewModel.AddUserOrContact();
        }

        private async void FindRoom()
        {
            await NavigateToUsers(false);
        }

        private async void ShowAttachments()
        {
            await NavigateToAttachments(_attachments, Meeting);
        }

        private async void SendEmail()
        {
            await UI.DisplayAndExecuteAction(GetString("MailOperationCaption"), new Dictionary<string, Action>
            {
                [GetString("ReplyAllOption")] = SendReplyAll,
                [GetString("ForwardOption")] = SendForward,
                [GetString("LateOption")] = SendLate,
            });
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

        private async void SelectFile()
        {
            await UI.NavigateTo("Files", null);
        }

        private class TimeSlot
        {
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }

            public static TimeSlot Parse(MeetingTimeCandidate mtc)
            {
                return new TimeSlot
                {
                    Start = ParseTimeSlot(mtc.MeetingTimeSlot.Start),
                    End = ParseTimeSlot(mtc.MeetingTimeSlot.End),
                };
            }

            private static TimeSpan ParseTimeSlot(MeetingTimeSlot.TimeDescriptor dateTime)
            {
                var time = DateTime.Parse(dateTime.Time);
                time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
                time = time.ToLocalTime();

                return time.TimeOfDay;
            }
        }
    }
}
