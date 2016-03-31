using Prism.Events;

namespace MeetingManager.Models
{
    class AcceptDeclineEvent : PubSubEvent<InvitationResponsePayload>
    {
    }
}
