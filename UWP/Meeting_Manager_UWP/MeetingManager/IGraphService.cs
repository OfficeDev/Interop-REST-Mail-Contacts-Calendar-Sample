//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingManager
{
    public interface IGraphService
    {
        Task<User> GetUser();
        Task<IEnumerable<Meeting>> GetCalendarEvents(DateTimeOffset startDate, DateTimeOffset endDate);
        Task<Meeting> GetEvent(string eventId);
        Task<Meeting> CreateEvent(Meeting meeting);
        Task<Meeting> UpdateEvent(Meeting meeting);
        Task CancelEvent(string eventId);
        Task<EventMessage> CreateInvitationResponse(Meeting meeting, string action);
        Task<bool> UpdateAndSendMessage(Message message, string comment, IEnumerable<Message.Recipient> recipients);
        Task DeleteDraftMessage(string messageid);
        Task AcceptOrDecline(string eventId, string action, string comment, bool sendResponse=true);
        Task<int> GetContactsCount();
        Task<IEnumerable<Contact>> GetContacts(int pageIndex, int pageSize);
        Task<byte[]> GetContactPhoto(string contactId);
        Task<IEnumerable<MeetingTimeCandidate>> FindMeetingTimes(Meeting meeting);

        IUserPager GetUserPager(int pageSize, string filter, bool getHumans);
    }

    public interface IUserPager
    {
        bool HasNextPage { get; }
        bool HasPrevPage { get; }
        Task<IEnumerable<User>> GetNextPage(bool next);
    }
}
