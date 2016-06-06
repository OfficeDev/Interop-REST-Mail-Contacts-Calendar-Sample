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
    class DetailsPageViewModel : ViewModel
    {
        private const string KindEdit = "edit";
        private const string KindSend = "send";
        private const string KindSilent = "silent";

        private string _description;
        private ObservableCollection<Attendee> _attendees;
        private string _dateTimeDescription;
        private bool _isOrganizer;

        private Meeting _meeting;

        public DetailsPageViewModel()
        {
            EditCommand = new DelegateCommand(EditMeeting);
            ReplyCommand = new DelegateCommand(SendReply);
            ReplyAllCommand = new DelegateCommand(SendReplyAll);
            ForwardCommand = new DelegateCommand(SendForward);
            LateCommand = new DelegateCommand(SendLate);
            AcceptCommand = new DelegateCommand<string>(AcceptMeeting);
            DeclineCommand = new DelegateCommand<string>(DeclineMeeting);
            TentativeCommand = new DelegateCommand<string>(TentativeMeeting);

            GetEvent<MeetingUpdatedEvent>().Subscribe(OnMeetingUpdate);
        }

        public DelegateCommand EditCommand { get; }
        public DelegateCommand ReplyCommand { get; }
        public DelegateCommand ReplyAllCommand { get; }
        public DelegateCommand ForwardCommand { get; }
        public DelegateCommand LateCommand { get; }
        public DelegateCommand<string> AcceptCommand { get; }
        public DelegateCommand<string> DeclineCommand { get; }
        public DelegateCommand<string> TentativeCommand { get; }

        [RestorableState]
        public Meeting Meeting
        {
            get { return _meeting; }
            private set { SetProperty(ref _meeting, value); }
        }

        public bool IsOrganizer
        {
            get { return _isOrganizer; }
            private set { SetProperty(ref _isOrganizer, value); }
        }

        public string Location
        {
            get
            {
                var location = _meeting.Location.DisplayName;

                return string.IsNullOrEmpty(location) ? GetString("NoLocation") : location;
            }
        }

        public string Description
        {
            get { return _description; }
            private set { SetProperty(ref _description, value); }
        }

        public string DateTimeDescription
        {
            get { return _dateTimeDescription; }
            private set { SetProperty(ref _dateTimeDescription, value); }
        }

        public ObservableCollection<Attendee> Attendees
        {
            get { return _attendees; }
            private set { SetProperty(ref _attendees, value); }
        }

        public bool IsContentText { get; set; }

        public string Organizer => _meeting.OrganizerName;

        public bool HasAttendees => Attendees.Any();

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                base.OnNavigatedTo(e, viewModelState);
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                Meeting = Deserialize<Meeting>((string)e.Parameter);
            }

            Populate();
        }

        private void Populate()
        {
            IsContentText = _meeting.IsContentText;

            DateTimeDescription = BuildDateTimeDescription(_meeting);

            Attendees = new ObservableCollection<Attendee>(_meeting.Attendees);
            IsOrganizer = _meeting.IsOrganizer;
        }

        private string BuildDateTimeDescription(Meeting meeting)
        {
            string date;

            if (meeting.Recurrence != null)
            {
                date = DateTimeUtils.BuildRecurrentDate(meeting.Recurrence);
            }
            else
            {
                DateTime dateTime;

                if (meeting.IsAllDay)
                {
                    dateTime = meeting.Start.DateTime;
                }
                else
                {
                    dateTime = meeting.Start.ToLocalTime();
                }

                date = string.Format("{0:dd MMM yyyy}", dateTime);
            }

            string time = meeting.IsAllDay ?
                GetString("AllDayPart") :
                string.Format(GetString("FromToPart"),
                                meeting.Start.ToLocalTime(),
                                meeting.End.ToLocalTime());

            return $"{date} {time}";
        }

        private async void EditMeeting()
        {
            await NavigateTo("Edit", _meeting);
        }

        private async void SendReply()
        {
            await NavigateToEmail(OData.Reply);
        }

        private async void SendReplyAll()
        {
            await NavigateToEmail(OData.ReplyAll);
        }

        private async void SendForward()
        {
            await NavigateToEmail(OData.Forward);
        }

        private async Task NavigateToEmail(string action, string comment=null)
        {
            await base.NavigateToEmail(Meeting, action, comment);
        }

        private async void SendLate()
        {
            await SendRunningLate(Meeting);
        }

        private void AcceptMeeting(string kind)
        {
            VerifyKind(kind);
            AcceptOrDecline(OData.Accept, kind);
        }

        private void VerifyKind(string kind)
        {
            switch (kind.ToLower())
            {
                case KindEdit:
                case KindSend:
                case KindSilent:
                    return;
                default:
                    throw new ArgumentException("Invalid kind: " + kind);
            }
        }

        private void DeclineMeeting(string kind)
        {
            VerifyKind(kind);
            AcceptOrDecline(OData.Decline, kind);
        }

        private void TentativeMeeting(string kind)
        {
            VerifyKind(kind);
            AcceptOrDecline(OData.TentativelyAccept, kind);
        }

        private void AcceptOrDecline(string action, string kind)
        {
            switch (kind.ToLower())
            {
                case KindEdit:
                    EditAndSendAcceptOrDecline(action);
                    break;
                case KindSend:
                    SendAcceptOrDecline(action);
                    break;
                case KindSilent:
                    SilentlyAcceptOrDecline(action);
                    break;
            }
        }

        private async void EditAndSendAcceptOrDecline(string action)
        {
            await PromptAcceptOrDecline(action);
        }

        private async void SendAcceptOrDecline(string action)
        {
            await AcceptOrDeclineAndBack(action, string.Empty, sendResponse: true);
        }

        private async void SilentlyAcceptOrDecline(string action)
        {
            await AcceptOrDeclineAndBack(action, null, sendResponse: false);
        }

        private void OnMeetingUpdate(Meeting meeting)
        {
            Meeting = meeting;
            Populate();
        }

        private async Task AcceptOrDeclineAndBack(string action, string comment, bool sendResponse)
        {
            await OfficeService.AcceptOrDecline(Meeting.Id, action, comment, sendResponse);
            GoBack();
        }

        private async Task PromptAcceptOrDecline(string action)
        {
            var dlg = new Views.AcceptDeclineDialog();

            var parameter = Serialize(Tuple.Create(action, Meeting.Id));

            GetEvent<InitDialogEvent>().Publish(parameter);

            await dlg.ShowAsync();
        }
    }
}
