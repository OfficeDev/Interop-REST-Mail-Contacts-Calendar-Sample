//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

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
        public int MaxCandidates { get; set; }

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
        public bool IsRequired { get; set; }
        public List<Location> Locations { get; set; }
    }

    public class MeetingTimeCandidatesResult
    {
        public string EmptySuggestionsHint { get; set; }
        public List<MeetingTimeCandidate> MeetingTimeSlots { get; set; }
    }

    public class MeetingTimeCandidate
    {
        public List<AttendeeAvailability> AttendeeAvailability;
        public string OrganizerAvailability { get; set; }
        public double Confidence { get; set; }
        public MeetingTimeSlot MeetingTimeSlot { get; set; }

        [JsonIgnore]
        public string TimeSlot { get; set; }
        [JsonIgnore]
        public string TimeZone { get; set; }
    }

    public class AttendeeAvailability
    {
        public Attendee Attendee { get; set; }
        public string Availability { get; set; }
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
