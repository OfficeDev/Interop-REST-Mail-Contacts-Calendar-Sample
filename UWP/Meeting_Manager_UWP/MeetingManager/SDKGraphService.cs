//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingManager.Models;
using Microsoft.Graph;
using System.Diagnostics;
using System.Net.Http.Headers;

using MMM = MeetingManager.Models;
using System.IO;
using System.Net.Http;

namespace MeetingManager
{
    class SDKGraphService : IGraphService
    {
        private const string GraphUrl = "https://graph.microsoft.com";

        private readonly GraphServiceClient _graphClient;

        private class LogHttpProvider : IHttpProvider
        {
            private readonly HttpClient _httpClient;
            private readonly Logger _logger;

            public LogHttpProvider(Logger logger)
            {
                _httpClient = new HttpClient();
                _logger = logger;

                this.Serializer = new Serializer();
            }

            public ISerializer Serializer { get; private set; }

            public void Dispose()
            {
                _httpClient?.Dispose();
            }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                var response = await _httpClient.SendAsync(request);
                LogResponse(response);
                
                return response;
            }

            private async void LogResponse(HttpResponseMessage response)
            {
                var request = response.RequestMessage;

                var method = request.Method.Method;
                var uri = request.RequestUri.ToString();
                string requestBody = string.Empty;

                if (request.Content != null)
                {
                    requestBody = await request.Content.ReadAsStringAsync();
                }

                var statusCode = $"{(int)response.StatusCode} ({response.StatusCode.ToString()})";
                string responseBody = string.Empty;

                var mediaType = response.Content?.Headers?.ContentType.MediaType;

                if (mediaType != null && !mediaType.Contains("image"))
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                }

                _logger.LogHttp(method, uri, requestBody, request.Headers.ToString(),
                            statusCode, responseBody, response.Headers.ToString());
            }
        }

        public SDKGraphService(IAuthenticationService authenticationService, Logger logger)
        {
            try
            {
                _graphClient = new GraphServiceClient(
                    GraphUrl + "/v1.0",
                    new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            var token = await authenticationService.GetTokenAsync(GraphUrl);
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        }),
                    new LogHttpProvider(logger));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not create a graph client: " + ex.Message);
                throw;
            }
        }

        public async Task AcceptOrDecline(string eventId, string action, string comment, bool sendResponse = true)
        {
            var builder = _graphClient.Me.Events[eventId];

            switch (action.ToLower())
            {
                case OData.Accept:
                    await builder.Accept(comment, sendResponse).Request().PostAsync();
                    break;
                case OData.Decline:
                    await builder.Decline(comment, sendResponse).Request().PostAsync();
                    break;
                case OData.TentativelyAccept:
                    await builder.TentativelyAccept(comment, sendResponse).Request().PostAsync();
                    break;
            }
        }

        public async Task<MMM.EventMessage> CreateInvitationResponse(Meeting meeting, string action)
        {
            var invite = await GetEventInvitation(meeting);

            if (invite == null)
            {
                return null;
            }

            var builder = _graphClient.Me.Messages[invite.Id];
            Microsoft.Graph.Message message = null;

            switch (action)
            {
                case OData.Reply:
                    message = await builder.CreateReply().Request().PostAsync();
                    break;
                case OData.ReplyAll:
                    message = await builder.CreateReplyAll().Request().PostAsync();
                    break;
                case OData.Forward:
                    message = await builder.CreateForward().Request().PostAsync();
                    break;
            }

            return message.ConvertObject<MMM.EventMessage>();
        }

        private async Task<Microsoft.Graph.EventMessage> GetEventInvitation(Meeting meeting)
        {
            var builder = meeting.IsOrganizer ?
                                            _graphClient.Me.MailFolders.SentItems :
                                            _graphClient.Me.MailFolders.Inbox;

            var invites = await builder.Messages.Request().Filter(BuildFilter(meeting)).GetAsync();

            var orderedInvites = invites.OrderBy(x => x.CreatedDateTime);

            var invite = orderedInvites.OfType<Microsoft.Graph.EventMessage>().FirstOrDefault();

            return invite;
        }

        private string BuildFilter(Meeting meeting)
        {
            var sb = new StringBuilder();
            
            sb.AppendFormat($"Subject eq '{meeting.Subject}'");
            AddTimeStamp(sb, meeting);

            return sb.ToString();
        }

        private void AddTimeStamp(StringBuilder sb, Meeting meeting)
        {
            // Use CreatedDateTime as an additional condition, but give it a little offset
            sb.Append(" and ");
            sb.Append("CreatedDateTime").Append(" gt ");
            var createdDateTime = meeting.CreatedDateTime.DateTime - TimeSpan.FromMinutes(1);
            sb.Append(DateTimeUtils.DateToFullApiUtcString(createdDateTime));
        }

        public async Task DeleteDraftMessage(string messageid)
        {
            await _graphClient.Me.MailFolders.Drafts.Messages[messageid].Request().DeleteAsync();
        }

        public async Task<bool> UpdateAndSendMessage(MMM.Message message)
        {
            var newMessage = message.ConvertObject<Microsoft.Graph.Message>();

            if (await _graphClient.Me.Messages[message.Id].Request().UpdateAsync(newMessage) == null)
            {
                return false;
            }

            await _graphClient.Me.Messages[message.Id].Send().Request().PostAsync();

            return true;
        }

        public async Task<MMM.User> GetUser()
        {
            var user = await _graphClient.Me.Request().GetAsync();

            return user.ConvertObject<MMM.User>();
        }

        public async Task<IEnumerable<Meeting>> GetCalendarEvents(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var options = new List<Option>
            {
                new QueryOption("startDateTime", startDate.Date.ToString("s")),
                new QueryOption("endDateTime", endDate.Date.ToString("s")),
            };

            var items = await _graphClient.Me.CalendarView.Request(options)
                                    .OrderBy("start/dateTime")
                                    .Top(100)
                                    .GetAsync();

            return items.Select(ev => ev.ConvertObject<Meeting>());
        }

        public async Task<Meeting> GetEvent(string eventId)
        {
            var item = await _graphClient.Me.Events[eventId].Request().GetAsync();

            return item.ConvertObject<Meeting>();
        }

        public async Task CancelEvent(string eventId)
        {
            await _graphClient.Me.Events[eventId].Request().DeleteAsync();
        }

        public async Task<Meeting> CreateEvent(Meeting meeting)
        {
            var newEvent = meeting.ConvertObject<Event>();

            var createdEvent = await _graphClient.Me.Events.Request().AddAsync(newEvent);

            return createdEvent.ConvertObject<Meeting>();
        }

        public async Task<Meeting> UpdateEvent(Meeting meeting)
        {
            var newEvent = meeting.ConvertObject<Event>();

            var updatedEvent = await _graphClient.Me.Events[meeting.Id].Request().UpdateAsync(newEvent);

            return updatedEvent.ConvertObject<Meeting>();
        }

        public async Task<byte[]> GetContactPhoto(string contactId)
        {
            try
            {
                var photoStream = await _graphClient.Me.Contacts[contactId].Photo.Content.Request().GetAsync();
                byte[] photoBytes;

                if (photoStream is MemoryStream)
                {
                    photoBytes = (photoStream as MemoryStream).ToArray();
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        photoStream.CopyTo(ms);
                        photoBytes = ms.ToArray();
                    }
                }

                return photoBytes;
            }
            catch
            {
                // Photo might not exist
                return null;
            }
        }

        public async Task<IEnumerable<MMM.Contact>> GetContacts(int pageIndex, int pageSize)
        {
            var contacts = await _graphClient.Me.Contacts.Request()
                                    .Top(pageSize)
                                    .Skip(pageSize * pageIndex)
                                    .OrderBy("DisplayName")
                                    .GetAsync();

            return contacts.Select(c => c.ConvertObject<MMM.Contact>());
        }

        public async Task<int> GetContactsCount()
        {
            var contacts = await _graphClient.Me.Contacts.Request().Top(100).GetAsync();

            // Assumption: there is just one page of contacts
            return contacts.Count;
        }

        public IUserPager GetUserPager(int pageSize, string filter, bool getHumans)
        {
            return new SDKUserPager(pageSize, filter, getHumans, _graphClient);
        }

        public async Task<IEnumerable<MMM.DriveItem>> GetDriveItems(string folderId, int pageIndex, int pageSize)
        {
            IDriveItemChildrenCollectionPage items;
            var builder = _graphClient.Me.Drive;

            if (string.IsNullOrEmpty(folderId))
            {
                items = await builder.Root.Children.Request().GetAsync();
            }
            else
            {
                items = await builder.Items[folderId].Children.Request().GetAsync();
            }

            return items.Select(i => i.ConvertObject<MMM.DriveItem>());
        }

        public async Task<byte[]> GetDriveItemContent(string id)
        {
            var itemStream = await _graphClient.Me.Drive.Items[id].Content.Request().GetAsync();

            byte[] itemBytes;

            if (itemStream is MemoryStream)
            {
                itemBytes = (itemStream as MemoryStream).ToArray();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    itemStream.CopyTo(ms);
                    itemBytes = ms.ToArray();
                }
            }

            return itemBytes;
        }

        public async Task<IEnumerable<MMM.FileAttachment>> GetEventAttachments(string eventId, int pageIndex, int pageSize)
        {
            var items = await _graphClient.Me.Events[eventId].Attachments.Request()
                                .Top(pageSize)
                                .Skip(pageSize * pageIndex)
                                .GetAsync();

            return items.Select(i => i.ConvertObject<MMM.FileAttachment>());
        }

        public async Task<MMM.FileAttachment> AddEventAttachment(string eventId, MMM.FileAttachment fileAttachment)
        {
            var attachment = fileAttachment.ConvertObject<Attachment>();

            var item = await _graphClient.Me.Events[eventId].Attachments.Request().AddAsync(attachment);

            return attachment.ConvertObject<MMM.FileAttachment>();
        }

        public async Task DeleteEventAttachment(string eventId, string attachmentId)
        {
            await _graphClient.Me.Events[eventId].Attachments[attachmentId].Request().DeleteAsync();
        }

        public async Task DeleteDriveItem(string id)
        {
            await _graphClient.Me.Drive.Items[id].Request().DeleteAsync();
        }

        public Task<IEnumerable<MeetingTimeCandidate>> FindMeetingTimes(Meeting meeting)
        {
            throw new NotImplementedException();
        }

        private class SDKUserPager : IUserPager
        {
            private readonly int _pageSize;
            private readonly string _filter;
            private readonly bool _getHumans;
            private readonly GraphServiceClient _graphClient;

            private IGraphServiceUsersCollectionRequest _next;

            public SDKUserPager(int pageSize, string filter, bool getHumans, GraphServiceClient graphClient)
            {
                _pageSize = pageSize;
                _filter = filter;
                _getHumans = getHumans;
                _graphClient = graphClient;
            }

            public async Task<IEnumerable<MMM.User>> GetNextPage(bool next)
            {
                IGraphServiceUsersCollectionPage users = null;

                // We only support forward paging
                if (next)
                {
                    if (_next == null)
                    {
                        users = await _graphClient.Users.Request()
                                            .Top(_pageSize)
                                            .Filter(BuildFilter())
                                            .GetAsync();
                    }
                    else
                    {
                        users = await _next.GetAsync();
                    }
                }

                _next = users.NextPageRequest;

                return users.Select(user => user.ConvertObject<MMM.User>());
            }

            public bool HasNextPage => _next != null;

            public bool HasPrevPage => false;

            private string BuildFilter()
            {
                return _getHumans ? BuildHumansFilter() : BuildRoomsFilter();
            }

            private string BuildHumansFilter()
            {
                var sb = new StringBuilder();
                AddFilter(sb);
                return sb.ToString();
            }

            private string BuildRoomsFilter()
            {
                var sb = new StringBuilder();
                // For rooms, we are making assumption about their 'givenName' property.
                AddStartsWith(sb, "Conf Room", "givenName");
                AddFilter(sb);

                return sb.ToString();
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
