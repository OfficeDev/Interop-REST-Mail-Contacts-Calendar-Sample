using Newtonsoft.Json;
using System.Collections.Generic;

namespace MeetingManager.Models
{
    public class MeetingTimes
    {
        public List<Attendee> Attendees { get; set; }
        public TimeConstraint TimeConstraint { get; set; }
        public LocationConstraint LocationConstraint { get; set; }
        public string MeetingDuration { get; set; }

        public class Attendee
        {
            public EmailAddress EmailAddress { get; set; }
        }
    }

    public class TimeConstraint
    {
        public List<MeetingTimeSlot> Timeslots { get; set; }
    }

    public class LocationConstraint
    {
        public List<Location> Locations { get; set; }
    }

    public class MeetingTimeCandidates
    {
        public List<MeetingTimeCandidate> Value { get; set; }
    }

    public class MeetingTimeCandidate
    {
        public MeetingTimeSlot MeetingTimeSlot { get; set; }

        [JsonIgnore]
        public string TimeSlot { get; set; }
        [JsonIgnore]
        public string TimeZone { get; set; }
    }

    public class MeetingTimeSlot
    {
        public TimeDescriptor Start { get; set; }
        public TimeDescriptor End { get; set; }

        public class TimeDescriptor
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string TimeZone { get; set; }
        }
    }
}
