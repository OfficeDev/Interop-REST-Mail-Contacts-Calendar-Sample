using MeetingManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task<IEnumerable<MeetingTimeCandidate>> GetMeetingTimeCandidates(Meeting meeting, string startTime, string endTime);

        IUserPager GetUserPager(int pageSize, string filter, bool getHumans);
    }

    public interface IUserPager
    {
        bool HasNextPage();
        bool HasPrevPage();
        Task<IEnumerable<User>> GetNextPage(bool next);
    }
}
