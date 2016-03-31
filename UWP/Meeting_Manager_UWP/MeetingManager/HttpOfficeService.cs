using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingManager.Models;
using System.Collections.ObjectModel;
using Windows.Web.Http;

namespace MeetingManager
{
    class HttpOfficeService : IOfficeService
    {
        public async Task<User> GetUser()
        {
            var user = await new HttpHelper().GetItemAsync<User>(string.Empty);

            return user;
        }

        public async Task<IEnumerable<Meeting>> GetCalendarEvents(DateTimeOffset date)
        {
            string uri = BuildCalendarUri(date, date.AddDays(1));

            var items = await new HttpHelper().GetItemsAsync<Meeting>(uri);

            return items ?? new List<Meeting>();
        }

        private String BuildCalendarUri(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            string start = startDate.ToString("O");
            string end = endDate.ToString("O");

            string uri = string.Format("calendarview?startDateTime={0}&endDateTime={1}", start, end);
            uri += "&$orderby=start/dateTime";

            return uri;
        }

        public async Task<Meeting> GetEvent(string eventId)
        {
            string uri = "events/" + eventId;

            var meeting = await new HttpHelper().GetItemAsync<Meeting>(uri);

            return meeting;
        }

        public async Task<Meeting> CreateEvent(Meeting meeting)
        {
            string uri = "events/";

            var newMeeting = await new HttpHelper().PostItemAsync<Meeting>(uri, meeting);

            return newMeeting;
        }

        public async Task<Meeting> UpdateEvent(Meeting meeting)
        {
            string uri = "events/" + meeting.Id;

            var newMeeting = await new HttpHelper().PatchItemAsync<Meeting>(uri, meeting);

            return newMeeting;
        }

        public IUserPager GetUserPager(int pageSize, string filter, bool getHumans)
        {
            return new HttpUserPager(pageSize, filter, getHumans);
        }

        public async Task<bool> UpdateAndSendMessage(Message message, string comment, IEnumerable<Message.Recipient> recipients)
        {
            message.Body.Content = comment + message.Body.Content;

            if (recipients != null)
            {
                message.ToRecipients = new List<Message.Recipient>(recipients);
            }

            string uri = "messages/" + message.Id;
            var helper = new HttpHelper();

            if (await helper.PatchItemAsync(uri, message) == null)
            {
                return false;
            }

            await helper.PostItemVoidAsync<Message>(uri + '/' + OData.Send);
            return true;
        }

        public async Task DeleteDraftMessage(string messageId)
        {
            string uri = "MailFolders/Drafts/messages/" + messageId;
            await new HttpHelper().DeleteItemAsync(uri);
        }

        public async Task AcceptOrDecline(string eventId, string action, string comment, bool send)
        {
            string uri = "events/" + eventId + '/' + action;

            var response = new InvitationResponse
            {
                Comment = comment,
                SendResponse = send
            };

            await new HttpHelper().PostItemAsync(uri, response);
        }

        public async Task<int> GetContactsCount()
        {
            string uri = "contacts/$count";
            return await new HttpHelper().GetItemAsync<int>(uri);
        }

        public async Task<IEnumerable<Contact>> GetContacts(int pageIndex, int pageSize)
        {
            string uri = string.Format("contacts?$top={0}&$skip={1}&$orderby=DisplayName", pageSize, pageSize * pageIndex);

            return await new HttpHelper().GetItemsAsync<Contact>(uri);
        }

        public async Task<byte[]> GetContactPhoto(string contactId)
        {
            string uri = string.Format("contacts/{0}/photo/$value", contactId);

            return await new HttpHelperPhoto().GetItemAsync<byte[]>(uri);
        }

        private class HttpHelperPhoto : HttpHelper
        {
            protected override void HandleFailure(string errorMessage, HttpStatusCode errorCode)
            {
                if (errorCode != HttpStatusCode.NotFound)
                {
                    base.HandleFailure(errorMessage, errorCode);
                }
            }
        }

        public async Task<IEnumerable<MeetingTimeCandidate>> GetMeetingTimeCandidates(Meeting meeting, string startTime, string endTime)
        {
            var body = BuildRequestBody(meeting, startTime, endTime);
            string uri = "https://outlook.office365.com/api/beta/me/findmeetingtimes";

            var candidates = await new HttpHelper().PostItemAsync<MeetingTimes, MeetingTimeCandidates> (uri, body);

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

            foreach (var a in meeting.Attendees)
            {
                if (!a.EmailAddress.IsEqualTo(meeting.Organizer.EmailAddress))
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

            string responseUri = BuildResponseUri(invite.Id, action);

            var reply = await new HttpHelper().PostItemAsync<EventMessage>(responseUri);

            return reply;
        }

        private String BuildResponseUri(string messageId, string action)
        {
            String uri = "messages/" + messageId + '/';

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

            var invites = await new HttpHelper().GetItemsAsync<EventMessage>(uri);

            var orderedInvites = invites.OrderBy(x => x.CreatedDateTime);

            var invite = orderedInvites.FirstOrDefault(x => x.Type.EqualsCaseInsensitive("#microsoft.graph.eventMessage"));

            return invite;
        }

        private string BuildInvitationsUri(Meeting meeting)
        {
            StringBuilder sb = new StringBuilder("MailFolders");
            sb.Append(meeting.IsOrganizer ? "/SentItems/" : "/Inbox/");
            sb.Append("Messages");
            sb.Append("?$filter=");

            BuildFilter(sb, meeting);

            return sb.ToString();
        }

        private void BuildFilter(StringBuilder sb, Meeting meeting)
        {
            sb.Append(string.Format("Subject eq '{0}'", meeting.Subject));
            AddTimeStamp(sb, meeting);
        }

        private void AddTimeStamp(StringBuilder sb, Meeting meeting)
        {
            sb.Append(" and ");
            sb.Append("CreatedDateTime").Append(" gt ");
            sb.Append(meeting.CreatedDateTime);
        }
    }

    class HttpUserPager : IUserPager
    {
        private readonly int _pageSize;
        private readonly string _filter;
        private readonly bool _getHumans;
        private readonly string _firstPageUri;
        private string _nextPageUri;
        private string _prevPageUri;
        private int _curPage = -1;

        public HttpUserPager(int pageSize, string filter, bool getHumans)
        {
            _pageSize = pageSize;
            _filter = filter;
            _getHumans = getHumans;

            _nextPageUri = _firstPageUri = BuildUri();
        }

        public async Task<IEnumerable<ADUser>> GetNextPage(bool next)
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

            var list = await new HttpHelper().GetItemAsync<HttpHelper.ODataList<ADUser>>(_nextPageUri);

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

            String skiptoken = Utils.GetToken(nextLink, "$skiptoken");
            if (skiptoken == null) {
                return null;
            }

            String nextUri = Utils.StripToken(currentUri, "&$skiptoken");
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
            sb.Append(string.Format("$top={0}", _pageSize));

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

        private void AddStartsWith(StringBuilder sb, String filter, String property)
        {
            sb.Append(string.Format("startswith({0},'{1}')", property, filter));
        }
    }
}
