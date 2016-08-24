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
    class DetailsPageViewModel : BaseViewModel
    {
        private const string KindEdit = "edit";
        private const string KindSend = "send";
        private const string KindSilent = "silent";

        private IEnumerable<FileAttachment> _attachments;

        public Command ReplyCommand => new Command(() => NavigateToEmail(OData.Reply));
        public Command ReplyAllCommand => new Command(() => NavigateToEmail(OData.ReplyAll));
        public Command ForwardCommand => new Command(() => NavigateToEmail(OData.Forward));
        public Command LateCommand => new Command(() => SendRunningLate(Meeting));
        public Command<string> AcceptCommand => new Command<string>((kind) => AcceptOrDecline(OData.Accept, kind));
        public Command<string> DeclineCommand => new Command<string>((kind) => AcceptOrDecline(OData.Decline, kind));
        public Command<string> TentativeCommand => new Command<string>((kind) => AcceptOrDecline(OData.TentativelyAccept, kind));

        public Command ShowAttachmentsCommand => new Command(() => NavigateToAttachments(_attachments, Meeting));

        [RestorableState]
        public Meeting Meeting { get; private set; }

        public string Location
        {
            get
            {
                var location = Meeting.Location.DisplayName;

                return string.IsNullOrEmpty(location) ? GetString("NoLocation") : location;
            }
        }

        public string DateTimeDescription { get; private set; }

        public ObservableCollection<Attendee> Attendees { get; private set; }

        public string Organizer => Meeting.OrganizerName;

        public bool HasAttachments => _attachments?.Any() == true;

        protected override async void OnNavigatedTo(object parameter)
        {
            if (parameter != null)
            {
                Meeting = JSON.Deserialize<Meeting>(parameter);
                OnPropertyChanged(() => Meeting);

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
            DateTimeDescription = BuildDateTimeDescription(Meeting);

            Attendees = new ObservableCollection<Attendee>(Meeting.Attendees);
            Attendees.ForEach(a => a.OrganizerAddress = Meeting.Organizer?.EmailAddress.Address);

            OnPropertyChanged(() => Attendees);
            OnPropertyChanged(() => DateTimeDescription);
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

        private void NavigateToEmail(string action)
        {
            NavigateToEmail(Meeting, action, comment: null);
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
                default:
                    throw new ArgumentException("Invalid kind: " + kind);
            }
        }

        private void EditAndSendAcceptOrDecline(string action)
        {
            PromptAcceptOrDecline(action);
        }

        private void SendAcceptOrDecline(string action)
        {
            AcceptOrDeclineAndBack(action, string.Empty, sendResponse: true);
        }

        private void SilentlyAcceptOrDecline(string action)
        {
            AcceptOrDeclineAndBack(action, null, sendResponse: false);
        }

        private async void AcceptOrDeclineAndBack(string action, string comment, bool sendResponse)
        {
            using (new Loading(this))
            {
                await GraphService.AcceptOrDecline(Meeting.Id, action, comment, sendResponse);
            }
            GoBack();
        }

        private void PromptAcceptOrDecline(string action)
        {
            UI.NavigateTo("AcceptDecline", Tuple.Create(action, Meeting.Id));
        }
    }
}
