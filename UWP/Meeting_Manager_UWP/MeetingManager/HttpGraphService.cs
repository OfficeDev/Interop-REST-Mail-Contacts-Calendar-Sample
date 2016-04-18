using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingManager.Models;
using Windows.Web.Http;

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

        public IUserPager GetUserPager(int pageSize, string filter, bool getHumans)
        {
            return new HttpUserPager(pageSize, filter, getHumans, GetHttpHelper());
        }

        public async Task<bool> UpdateAndSendMessage(Message message, string comment, IEnumerable<Message.Recipient> recipients)
        {
            message.Body.Content = comment + message.Body.Content;

            if (recipients != null)
            {
                message.ToRecipients = new List<Message.Recipient>(recipients);
            }

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

        public async Task<IEnumerable<MeetingTimeCandidate>> GetMeetingTimeCandidates(Meeting meeting, string startTime, string endTime)
        {
            var body = BuildRequestBody(meeting, startTime, endTime);
            string uri = "https://outlook.office365.com/api/beta/me/findmeetingtimes";

            var candidates = await GetHttpHelper().PostItemAsync<MeetingTimes, MeetingTimeCandidates> (uri, body);

            return candidates == null ? Enumerable.Empty<MeetingTimeCandidate>(): candidates.Value;
        }

        private MeetingTimes BuildRequestBody(Meeting meeting, string startTime, string endTime)
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
                    Locations = new List<Location>()
                }
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

            var date = meeting.Start.ToLocalTime();
            var dateString = date.DateToApiString();

            var timeSlot = new MeetingTimeSlot
            {
                Start = new MeetingTimeSlot.TimeDescriptor
                {
                    Date = dateString,
                    Time = startTime,
                    TimeZone = TimeZoneInfo.Local.Id
                },
                End = new MeetingTimeSlot.TimeDescriptor
                {
                    Date = dateString,
                    Time = endTime,
                    TimeZone = TimeZoneInfo.Local.Id
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

            var orderedInvites = invites.OrderBy(x => x.CreatedDateTime);

            var invite = orderedInvites.FirstOrDefault(x => x.Type.EqualsCaseInsensitive("#microsoft.graph.eventMessage"));

            return invite;
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
    }

    class HttpUserPager : IUserPager
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

            var items = list.value;

            if (next)
            {
                ++_curPage;
            }
            else
            {
                --_curPage;
            }

            _nextPageUri = GetNextPageUri(_nextPageUri, list.NextLink);

            if (_nextPageUri != null && _nextPageUri.Contains("$skiptoken"))
            {
                _prevPageUri = _nextPageUri;
            }

            return items;
        }

        private string GetNextPageUri(string currentUri, string nextLink)
        {
            if (string.IsNullOrEmpty(nextLink)) {
                return null;
            }

            string skiptoken = Utils.GetToken(nextLink, "$skiptoken");
            if (skiptoken == null) {
                return null;
            }

            string nextUri = Utils.StripToken(currentUri, "&$skiptoken");
            nextUri = Utils.StripToken(nextUri, "&previous-page");

            if (nextUri.Last() != '&') {
                nextUri += '&';
            }

            nextUri += skiptoken;

            return nextUri;
        }

        public bool HasNextPage()
        {
            return _nextPageUri != null;
        }

        public bool HasPrevPage()
        {
            return _curPage > 0;
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
