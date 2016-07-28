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
    class DetailsPageViewModel : BaseViewModel
    {
        private const string KindEdit = "edit";
        private const string KindSend = "send";
        private const string KindSilent = "silent";

        private ObservableCollection<Attendee> _attendees;
        private string _dateTimeDescription;

        private Meeting _meeting;
        private IEnumerable<FileAttachment> _attachments;

        public Command EditCommand => new Command(EditMeeting);
        public Command ReplyCommand => new Command(SendReply);
        public Command SendEmailCommand => new Command(SendEmail);
        public Command ReplyAllCommand => new Command(SendReplyAll);
        public Command ForwardCommand => new Command(SendForward);
        public Command LateCommand => new Command(SendLate);
        public Command AcceptCommand => new Command(AcceptMeeting);
        public Command DeclineCommand => new Command(DeclineMeeting);
        public Command TentativeCommand => new Command(TentativeMeeting);
        public Command ShowAttachmentsCommand => new Command(ShowAttachments);

        public Meeting Meeting
        {
            get { return _meeting; }
            private set { SetProperty(ref _meeting, value); }
        }

        public string Location
        {
            get
            {
                var location = _meeting.Location.DisplayName;

                return string.IsNullOrEmpty(location) ? GetString("NoLocation") : location;
            }
        }

        public string Description => Meeting.Body.Content;

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

        public bool HasAttachments => _attachments != null && _attachments.Any();

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

        public override async void OnAppearing(object data)
        {
            if (data != null)
            {
                Meeting = JSON.Deserialize<Meeting>(data);

                using (new Loading(this))
                {
                    _attachments = await GraphService.GetEventAttachments(Meeting.Id, 0, 100);
                    OnPropertyChanged(() => HasAttachments);
                }
            }

            Populate();
        }

        private void Populate()
        {
            IsContentText = Meeting.Body.ContentType.EqualsCaseInsensitive("text");
            OnPropertyChanged(() => HtmlSource);
            OnPropertyChanged(() => Description);

            DateTimeDescription = BuildDateTimeDescription(_meeting);

            Attendees = new ObservableCollection<Attendee>(_meeting.Attendees);
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
            await UI.NavigateTo("Edit", _meeting);
        }

        private async void SendEmail()
        {
            var options = new Dictionary<string, Action>
            {
                [GetString("ReplyOption")] = SendReply,
                [GetString("ReplyAllOption")] = SendReplyAll,
                [GetString("ForwardOption")] = SendForward,
                [GetString("LateOption")] = SendLate,
            };

            await UI.DisplayAndExecuteAction(GetString("MailOperationCaption"), options);
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

        private async Task NavigateToEmail(string action, string comment = null)
        {
            await base.NavigateToEmail(Meeting, action, comment);
        }

        private async void SendLate()
        {
            await SendRunningLate(Meeting);
        }

        private async void AcceptDeclineChoice(Action<string> action)
        {
            var edit = GetString("EditResponseOption");
            var send = GetString("SendResponseOption");
            var silent = GetString("NoResponseOption");

            var operation = await UI.DisplayActions(GetString("AcceptDeclineCaption"), edit, send, silent);

            if (operation == edit)
            {
                action(KindEdit);
            }
            else if (operation == send)
            {
                action(KindSend);
            }
            else if (operation == silent)
            {
                action(KindSilent);
            }
        }

        private void AcceptMeeting()
        {
            AcceptDeclineChoice(AcceptMeeting);
        }

        private void DeclineMeeting()
        {
            AcceptDeclineChoice(DeclineMeeting);
        }

        private void TentativeMeeting()
        {
            AcceptDeclineChoice(TentativeMeeting);
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

        private void AcceptMeeting(string kind)
        {
            AcceptOrDecline(OData.Accept, kind);
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

        private async Task AcceptOrDeclineAndBack(string action, string comment, bool sendResponse)
        {
            await GraphService.AcceptOrDecline(Meeting.Id, action, comment, sendResponse);
            await UI.GoBack();
        }

        private async void ShowAttachments()
        {
            await NavigateToAttachments(_attachments, Meeting);
        }

        private async Task PromptAcceptOrDecline(string action)
        {
            await UI.NavigateTo("AcceptOrDecline", Tuple.Create(action, Meeting.Id));
        }
    }
}
