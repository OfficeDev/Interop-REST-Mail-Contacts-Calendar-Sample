//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MeetingManager
{
    class HttpGraphService : IGraphService
    {
        private const string EventsFolder = "events/";
        private const string Messages = "messages";
        private const string MessagesFolder = Messages + "/";
        private const string Contacts = "contacts";
        private const string ContactsFolder = Contacts + "/";

        private readonly IAuthenticationService _authService;
        private readonly Logger _logger;

        public HttpGraphService(IAuthenticationService authenticationService, Logger logger)
        {
            _authService = authenticationService;
            _logger = logger;
        }

        public async Task<User> GetUser()
        {
            return await GetHttpHelper().GetItemAsync<User>(string.Empty);
        }

        public async Task<IEnumerable<Meeting>> GetCalendarEvents(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            string uri = BuildCalendarUri(startDate, endDate);

            var items = await GetHttpHelper().GetItemsAsync<Meeting>(uri);

            return items ?? new List<Meeting>();
        }

        private string BuildCalendarUri(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            string start = startDate.Date.ToString("s");
            string end = endDate.Date.ToString("s");

            string uri = $"calendarview?startDateTime={start}&endDateTime={end}";
            uri += "&$orderby=start/dateTime";
            uri += "&$top=100";

            return uri;
        }

        public async Task<Meeting> GetEvent(string eventId)
        {
            string uri = EventsFolder + eventId;

            return await GetHttpHelper().GetItemAsync<Meeting>(uri);
        }

        public async Task<Meeting> CreateEvent(Meeting meeting)
        {
            string uri = EventsFolder;

            return await GetHttpHelper().PostItemAsync<Meeting>(uri, meeting);
        }

        public async Task<Meeting> UpdateEvent(Meeting meeting)
        {
            string uri = EventsFolder + meeting.Id;

            return await GetHttpHelper().PatchItemAsync<Meeting>(uri, meeting);
        }

        public async Task CancelEvent(string eventId)
        {
            string uri = EventsFolder + eventId;

            await GetHttpHelper().DeleteItemAsync(uri);
        }

        public async Task<bool> UpdateAndSendMessage(Message message)
        {
            string uri = MessagesFolder + message.Id;
            var helper = GetHttpHelper();

            if (await helper.PatchItemAsync(uri, message) == null)
            {
                return false;
            }

            await helper.PostItemVoidAsync<Message>(uri + '/' + OData.Send);
            return true;
        }

        public async Task DeleteDraftMessage(string messageId)
        {
            string uri = "MailFolders/Drafts/" + MessagesFolder + messageId;
            await GetHttpHelper().DeleteItemAsync(uri);
        }

        public async Task AcceptOrDecline(string eventId, string action, string comment, bool send)
        {
            string uri = EventsFolder + eventId + '/' + action;

            var response = new InvitationResponse
            {
                Comment = comment,
                SendResponse = send
            };

            await GetHttpHelper().PostItemAsync(uri, response);
        }

        public async Task<int> GetContactsCount()
        {
            string uri = ContactsFolder + "$count";
            return await GetHttpHelper().GetItemAsync<int>(uri);
        }

        public async Task<IEnumerable<Contact>> GetContacts(int pageIndex, int pageSize)
        {
            // Assumption: pageSize is less or equal to the service default page size
            string uri = Contacts +
                        $"?$top={pageSize}&$skip={pageSize * pageIndex}&$orderby=DisplayName";

            return await GetHttpHelper().GetItemsAsync<Contact>(uri);
        }

        public async Task<byte[]> GetContactPhoto(string contactId)
        {
            string uri = string.Format(ContactsFolder + "{0}/photo/$value", contactId);

            return await new HttpHelperPhoto(_authService, _logger).GetItemAsync<byte[]>(uri);
        }

        private HttpHelper GetHttpHelper()
        {
            return new HttpHelper(_authService, _logger);
        }

        private class HttpHelperPhoto : HttpHelper
        {
            public HttpHelperPhoto(IAuthenticationService authService, Logger logger) : base(authService, logger)
            {
            }

            protected override void HandleFailure(string errorMessage, HttpResponseMessage response)
            {
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    base.HandleFailure(errorMessage, response);
                }
            }
        }

        public async Task<IEnumerable<MeetingTimeCandidate>> FindMeetingTimes(Meeting meeting)
        {
            var body = BuildRequestBody(meeting);
            string uri = "https://graph.microsoft.com/beta/me/findmeetingtimes";

            var candidates = await GetHttpHelper().PostItemAsync<MeetingTimes, MeetingTimeCandidatesResult>(uri, body);

            if (candidates == null || candidates.MeetingTimeSlots == null)
            {
                return Enumerable.Empty<MeetingTimeCandidate>();
            }

            return candidates.MeetingTimeSlots;
        }

        private MeetingTimes BuildRequestBody(Meeting meeting)
        {
            var result = new MeetingTimes
            {
                MeetingDuration = "PT30M",
                Attendees = new List<MeetingTimes.Attendee>(),
                TimeConstraint = new TimeConstraint
                {
                    Timeslots = new List<MeetingTimeSlot>()
                },
                LocationConstraint = new LocationConstraint()
                {
                    IsRequired = false,
                    Locations = new List<Location>()
                },
                MaxCandidates = 20,
            };

            foreach (var a in meeting.Attendees ?? Enumerable.Empty<Attendee>())
            {
                if (meeting.Organizer == null || !a.EmailAddress.IsEqualTo(meeting.Organizer.EmailAddress))
                {
                    result.Attendees.Add(new MeetingTimes.Attendee
                    {
                        EmailAddress = a.EmailAddress
                    });
                }
            }

            var date = meeting.Start.DateTime;

            // From 8AM to 6PM local time
            var start = new DateTime(date.Year, date.Month, date.Day, 8, 0, 0, DateTimeKind.Local);
            var end = new DateTime(date.Year, date.Month, date.Day, 18, 0, 0, DateTimeKind.Local);

            start = start.ToUniversalTime();
            end = end.ToUniversalTime();

            var timeSlot = new MeetingTimeSlot
            {
                Start = new MeetingTimeSlot.TimeDescriptor
                {
                    Date = start.DateToApiString(),
                    Time = start.TimeOfDay.ToString(),
                    TimeZone = "UTC"
                },
                End = new MeetingTimeSlot.TimeDescriptor
                {
                    Date = end.DateToApiString(),
                    Time = end.TimeOfDay.ToString(),
                    TimeZone = "UTC"
                }
            };

            result.TimeConstraint.Timeslots.Add(timeSlot);

            if (!string.IsNullOrEmpty(meeting.Location.DisplayName))
            {
                result.LocationConstraint.Locations.Add(new Location
                {
                    DisplayName = meeting.Location.DisplayName
                });
            }

            return result;
        }

        public async Task<EventMessage> CreateInvitationResponse(Meeting meeting, string action)
        {
            var invite = await GetEventInvitation(meeting);

            if (invite == null)
            {
                return null;
            }

            string responseUri = BuildResponseUri(invite.Id, action);

            var reply = await GetHttpHelper().PostItemAsync<EventMessage>(responseUri);

            return reply;
        }

        private string BuildResponseUri(string messageId, string action)
        {
            string uri = MessagesFolder + messageId + '/';

            switch (action)
            {
                case OData.Reply:
                    uri += OData.CreateReply;
                    break;
                case OData.ReplyAll:
                    uri += OData.CreateReplyAll;
                    break;
                case OData.Forward:
                    uri += OData.CreateForward;
                    break;
            }
            return uri;
        }

        private async Task<EventMessage> GetEventInvitation(Meeting meeting)
        {
            string uri = BuildInvitationsUri(meeting);

            var invites = await GetHttpHelper().GetItemsAsync<EventMessage>(uri);

            // Put most recent invite on top
            var orderedInvites = invites.OrderByDescending(x => x.CreatedDateTime);

            return orderedInvites.FirstOrDefault(x => x.Type.EqualsCaseInsensitive("#microsoft.graph.eventMessage"));
        }

        private string BuildInvitationsUri(Meeting meeting)
        {
            StringBuilder sb = new StringBuilder("MailFolders");
            sb.Append(meeting.IsOrganizer ? "/SentItems/" : "/Inbox/");
            sb.Append(Messages);
            sb.Append("?$filter=");

            BuildFilter(sb, meeting);

            return sb.ToString();
        }

        private void BuildFilter(StringBuilder sb, Meeting meeting)
        {
            sb.AppendFormat("Subject eq '{0}'", meeting.Subject);
            AddTimeStamp(sb, meeting);
        }

        private void AddTimeStamp(StringBuilder sb, Meeting meeting)
        {
            // Use CreatedDateTime as an additional condition, but give it a little offset
            sb.Append(" and ");
            sb.Append("CreatedDateTime").Append(" gt ");
            var createdDateTime = meeting.CreatedDateTime.DateTime - TimeSpan.FromMinutes(1);
            sb.Append(DateTimeUtils.DateToFullApiUtcString(createdDateTime));
        }

        public IUserPager GetUserPager(int pageSize, string filter, bool getHumans)
        {
            return new HttpUserPager(pageSize, filter, getHumans, GetHttpHelper());
        }

        public async Task<IEnumerable<DriveItem>> GetDriveItems(string folderId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(folderId))
            {
                return await GetHttpHelper().GetItemsAsync<DriveItem>("drive/root/children");
            }
            else
            {
                return await GetHttpHelper().GetItemsAsync<DriveItem>($"drive/items/{folderId}/children");
            }
        }

        public async Task<byte[]> GetDriveItemContent(string id)
        {
            string uri = "drive/items/" + id + "/content";

            return await GetHttpHelper().GetItemAsync<byte[]>(uri);
        }

        public async Task DeleteDriveItem(string id)
        {
            string uri = "drive/items/" + id;

            await GetHttpHelper().DeleteItemAsync(uri);
        }

        public async Task<FileAttachment> AddEventAttachment(string eventId, FileAttachment attachment)
        {
            string uri = EventsFolder + eventId + "/attachments";

            return await GetHttpHelper().PostItemAsync<FileAttachment>(uri, attachment);
        }

        public async Task<IEnumerable<FileAttachment>> GetEventAttachments(string eventId, int pageIndex, int pageSize)
        {
            string uri = EventsFolder + eventId + "/attachments";

            return await GetHttpHelper().GetItemsAsync<FileAttachment>(uri);
        }

        public async Task DeleteEventAttachment(string eventId, string attachmentId)
        {
            string uri = EventsFolder + eventId + "/attachments/" + attachmentId;

            await GetHttpHelper().DeleteItemAsync(uri);
        }

        private class HttpUserPager : IUserPager
        {
            private readonly int _pageSize;
            private readonly string _filter;
            private readonly bool _getHumans;
            private readonly string _firstPageUri;
            private readonly HttpHelper _httpHelper;

            private string _nextPageUri;
            private string _prevPageUri;
            private int _curPage = -1;

            public HttpUserPager(int pageSize, string filter, bool getHumans, HttpHelper httpHelper)
            {
                _pageSize = pageSize;
                _filter = filter;
                _getHumans = getHumans;
                _httpHelper = httpHelper;

                _nextPageUri = _firstPageUri = BuildUri();
            }

            public bool HasNextPage => _nextPageUri != null;

            public bool HasPrevPage => _curPage > 0 && _getHumans && string.IsNullOrEmpty(_filter);

            public async Task<IEnumerable<User>> GetNextPage(bool next)
            {
                if (next == false)
                {
                    if (_curPage <= 1)
                    {
                        _nextPageUri = _firstPageUri;
                    }
                    else if (_prevPageUri != null && !_prevPageUri.Contains("previous-page"))
                    {
                        _nextPageUri = _prevPageUri + "&previous-page=true";
                    }
                }

                var list = await _httpHelper.GetItemAsync<HttpHelper.ODataList<User>>(_nextPageUri);

                var items = list?.value;

                if (next)
                {
                    ++_curPage;
                }
                else
                {
                    --_curPage;
                }

                _nextPageUri = GetNextPageUri(_nextPageUri, list?.NextLink);

                if (_nextPageUri != null && _nextPageUri.Contains("$skiptoken"))
                {
                    _prevPageUri = _nextPageUri;
                }

                return items;
            }

            private string GetNextPageUri(string currentUri, string nextLink)
            {
                if (string.IsNullOrEmpty(nextLink))
                {
                    return null;
                }

                string skiptoken = Utils.GetToken(nextLink, "$skiptoken");
                if (skiptoken == null)
                {
                    return null;
                }

                string nextUri = Utils.StripToken(currentUri, "&$skiptoken");
                nextUri = Utils.StripToken(nextUri, "&previous-page");

                if (nextUri[nextUri.Length - 1] != '&')
                {
                    nextUri += '&';
                }

                nextUri += skiptoken;

                return nextUri;
            }

            private string BuildUri()
            {
                string uri = "https://graph.microsoft.com/v1.0/users?";
                var sb = new StringBuilder(uri);
                sb.AppendFormat("$top={0}", _pageSize);

                if (_getHumans)
                {
                    BuildHumansQuery(sb);
                }
                else
                {
                    BuildRoomsQuery(sb);
                }

                AddFilter(sb);

                return sb.ToString();
            }

            private void BuildHumansQuery(StringBuilder sb)
            {
            }

            private void BuildRoomsQuery(StringBuilder sb)
            {
                sb.Append("&$filter=");
                // For rooms, we are making assumption about their 'givenName' property.
                AddStartsWith(sb, "Conf Room", "givenName");
            }

            private void AddFilter(StringBuilder sb)
            {
                if (string.IsNullOrEmpty(_filter))
                {
                    return;
                }

                if (!_getHumans)
                {
                    sb.Append(" and ");
                    sb.Append('(');
                    AddMoreNameFilters(sb, _filter);
                    sb.Append(')');
                }
                else
                {
                    sb.Append("&$filter=");
                    AddStartsWith(sb, _filter, "givenName");
                    sb.Append(" or ");
                    AddMoreNameFilters(sb, _filter);
                }
            }

            private void AddMoreNameFilters(StringBuilder sb, string filter)
            {
                AddStartsWith(sb, filter, "userPrincipalName");
                sb.Append(" or ");
                AddStartsWith(sb, filter, "displayName");
            }

            private void AddStartsWith(StringBuilder sb, string filter, string property)
            {
                sb.AppendFormat("startswith({0},'{1}')", property, filter);
            }
        }
    }
}
