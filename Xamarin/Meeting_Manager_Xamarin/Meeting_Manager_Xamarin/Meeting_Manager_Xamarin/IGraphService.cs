//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meeting_Manager_Xamarin
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
        Task<bool> UpdateAndSendMessage(Message message);
        Task DeleteDraftMessage(string messageid);
        Task AcceptOrDecline(string eventId, string action, string comment, bool sendResponse = true);
        Task<int> GetContactsCount();
        Task<IEnumerable<Contact>> GetContacts(int pageIndex, int pageSize);
        Task<byte[]> GetContactPhoto(string contactId);
        Task<IEnumerable<DriveItem>> GetDriveItems(string folderId, int pageIndex, int pageSize);
        Task<byte[]> GetDriveItemContent(string id);
        Task DeleteDriveItem(string id);
        Task<FileAttachment> AddEventAttachment(string eventId, FileAttachment attachment);
        Task<IEnumerable<FileAttachment>> GetEventAttachments(string eventId, int pageIndex, int pageSize);
        Task DeleteEventAttachment(string eventId, string attachmentId);
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
