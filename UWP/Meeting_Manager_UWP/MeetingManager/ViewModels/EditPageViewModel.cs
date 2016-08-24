//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using Prism.Windows.AppModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MeetingManager.ViewModels
{
    class EditPageViewModel : BaseViewModel
    {
        private ObservableCollection<Attendee> _attendees;
        private List<FileAttachment> _attachments;

        public EditPageViewModel()
        {
            UI.Subscribe<User>(UserSelected);
            UI.Subscribe<Room>((room) => LocationName = room.ToString());
            UI.Subscribe<Contact>(ContactSelected);
            UI.Subscribe<Meeting.EventRecurrence>(RecurrenceUpdated);
            UI.Subscribe<DriveItem>(AttachmentSelected);
            UI.Subscribe<FileAttachment>(AttachmentDeleted);
            UI.Subscribe<MeetingTimeCandidate>(TimeSlotSelected);
        }

        public Command SaveCommand => new Command(SaveMeetingAsync);
        public Command AttachCommand => new Command(() => UI.NavigateTo("Files"));
        public Command RecurrenceCommand => new Command(SetRecurrence);
        public Command AddUserCommand => new Command(() => NavigateToUsers(true));
        public Command AddContactCommand => new Command(() => NavigateToContacts());
        public Command FindRoomCommand => new Command(() => NavigateToUsers(false));
        public Command GetSuggestedTimeCommand => new Command(GetSuggestedTime, CanExecuteMeetingTimes);
        public Command ASAPCommand => new Command(ASAPMeeting, CanExecuteMeetingTimes);
        public Command<Attendee> DeleteAttendeeCommand => new Command<Attendee>(DeleteAttendee);
        public Command ReplyAllCommand => new Command(() => NavigateToEmail(OData.ReplyAll));
        public Command ForwardCommand => new Command(() => NavigateToEmail(OData.Forward));
        public Command LateCommand => new Command(() => SendRunningLate(Meeting));
        public Command ShowAttachmentsCommand => new Command(() => NavigateToAttachments(_attachments, Meeting));

        [RestorableState]
        public Meeting Meeting { get; private set; }

        public string Title => GetString(IsNewMeeting ? "CreateMeetingTitle" : "UpdateMeetingTitle");

        public bool HasAttachments => _attachments?.Any() == true;

        public bool IsNewMeeting => string.IsNullOrEmpty(Meeting.Id);

        public DateTimeOffset StartDate
        {
            get
            {
                return IsAllDay ? Meeting.Start.DateTime : Meeting.Start.ToLocalTime();
            }

            set
            {
                SetTime(Meeting.Start, value);

                if (!AreDatesValid())
                {
                    if (IsAllDay)
                    {
                        Meeting.End.DateTime = Meeting.Start.DateTime + TimeSpan.FromDays(1);
                    }
                    else
                    {
                        Meeting.End.DateTime = Meeting.Start.DateTime + TimeSpan.FromMinutes(30);
                        OnPropertyChanged(() => EndTime);
                    }
                    OnPropertyChanged(() => EndDate);
                }
            }
        }

        public DateTimeOffset EndDate
        {
            get
            {
                return IsAllDay ? Meeting.End.DateTime : Meeting.End.ToLocalTime();
            }

            set
            {
                SetTime(Meeting.End, value);

                if (!AreDatesValid())
                {
                    if (IsAllDay)
                    {
                        Meeting.Start.DateTime = Meeting.End.DateTime - TimeSpan.FromDays(1);
                    }
                    else
                    {
                        Meeting.Start.DateTime = Meeting.End.DateTime - TimeSpan.FromMinutes(30);
                        OnPropertyChanged(() => StartTime);
                    }
                    OnPropertyChanged(() => StartDate);
                }
            }
        }

        private bool AreDatesValid()
        {
            return Meeting.Start.DateTime.CompareTo(Meeting.End.DateTime) < 0;
        }

        private void SetTime(ZonedDateTime current, DateTimeOffset value)
        {
            if (IsAllDay)
            {
                current.DateTime = value.Date;
            }
            else
            {
                var local = current.ToLocalTime();
                var newTime = value.Date + local.TimeOfDay;

                current.DateTime = newTime.ToUtcTime();
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

        public ObservableCollection<Attendee> Attendees
        {
            get { return _attendees; }
            private set { SetCollectionProperty(ref _attendees, value); }
        }

        public string RecurrenceDate { get; private set; }

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
                OnPropertyChanged(() => StartDate);
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

            dateTime.DateTime = localDateTime.ToUtcTime();
        }

        protected override async void OnNavigatedTo(object parameter)
        {
            if (parameter != null)
            {
                Meeting = JSON.Deserialize<Meeting>(parameter);

                using (new Loading(this))
                {
                    _attachments = (await GraphService.GetEventAttachments(Meeting.Id, 0, 100))
                                    .ToList();
                    OnPropertyChanged(() => HasAttachments);
                }
            }
            else
            {
                Meeting = CreateNewMeeting();
                _attachments = new List<FileAttachment>();
            }

            BuildRecurrentDate();
            PopulateAttendees();

            OnPropertyChanged(() => Meeting);
        }

        private void BuildRecurrentDate()
        {
            if (IsSerial)
            {
                RecurrenceDate = DateTimeUtils.BuildRecurrentDate(Meeting.Recurrence);
                OnPropertyChanged(() => RecurrenceDate);
            }
        }

        private void PopulateAttendees()
        {
            Attendees = new ObservableCollection<Attendee>(Meeting.Attendees);
            Attendees.ForEach(a => a.OrganizerAddress = Meeting.Organizer?.EmailAddress.Address);

            OnPropertyChanged(() => Attendees);

            UpdateAttendeesRelatedProperties();
        }

        private void UpdateAttendeesRelatedProperties()
        {
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

            Meeting newMeeting;
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

                UI.Publish(newMeeting);
                GoBack();
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

        private void AttachmentDeleted(FileAttachment item)
        {
            var removed = _attachments.FirstOrDefault(x => x.Name == item.Name);
            _attachments.Remove(removed);

            OnPropertyChanged(() => HasAttachments);
        }

        private async void AttachmentSelected(DriveItem driveItem)
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

        private void DeleteAttendee(Attendee attendee)
        {
            if (attendee.IsOrganizer) return;

            int pos = Meeting.Attendees.IndexOf(x => x.EmailAddress.IsEqualTo(attendee.EmailAddress));

            Meeting.Attendees.RemoveAt(pos);

            PopulateAttendees();
        }

        private void SetRecurrence()
        {
            var recurrence = Meeting.Recurrence ?? CreateDefaultRecurrence();

            if (!recurrence.Range.Type.EqualsCaseInsensitive(OData.EndDate))
            {
                recurrence.Range.EndDate = new DateTime(DateTime.Now.Year, 12, 31).ToString();
            }

            UI.NavigateTo("Recurrence", recurrence);
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
            Meeting.Recurrence = recurrence;
            OnPropertyChanged(() => IsSerial);

            BuildRecurrentDate();
        }

        private void GetSuggestedTime()
        {
            UI.NavigateTo("TimeSlots", Meeting);
        }

        private async void ASAPMeeting()
        {
            var items = await GetAllTimeCandidates(Meeting);

            var slot = items.Select(x => TimeSlot.Parse(x))
                            .OrderBy(x => x.Start)
                            .FirstOrDefault(x => Meeting.Start.DateTime.Date + x.Start > DateTime.Now);

            if (slot == null)
            {
                await UI.MessageDialog(GetString("NoFreeSlots"));
            }
            else
            {
                SetTimeSlot(slot);
            }
        }

        private void TimeSlotSelected(MeetingTimeCandidate meetingTimeCandidate)
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

        private void NavigateToEmail(string action)
        {
            NavigateToEmail(Meeting, action, comment: null);
        }

        private bool CanExecuteMeetingTimes()
        {
            return !IsAllDay && App.Me.UseHttp;
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
