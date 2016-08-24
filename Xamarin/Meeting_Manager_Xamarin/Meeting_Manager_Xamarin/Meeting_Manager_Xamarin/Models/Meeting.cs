//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Meeting_Manager_Xamarin.Models
{
    public class Meeting
    {
        public Meeting()
        {
        }

        public string Id { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string SeriesMasterId { get; set; }

        public ZonedDateTime Start { get; set; }
        public ZonedDateTime End { get; set; }

        public Organizer Organizer { get; set; }

        public bool IsOrganizer { get; set; }

        public Body Body { get; set; }

        public EventRecurrence Recurrence { get; set; }

        public bool IsAllDay { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }

        public string OriginalStartTimeZone { get; set; }
        public string OriginalEndTimeZone { get; set; }

        [JsonIgnore]
        public string AttendeeList => string.Join(";\n", Attendees);

        [JsonIgnore]
        public bool IsContentText => Body.ContentType.EqualsCaseInsensitive("Text");

        [JsonIgnore]
        public string StartTime
        {
            get
            {
                if (IsAllDay)
                {
                    return ResMan.GetString("AllDayCaption");
                }
                else
                {
                    var localDateTime = Start.ToLocalTime();
                    return string.Format(ResMan.GetString("StartFormat"), localDateTime);
                }
            }
        }

        [JsonIgnore]
        public string OrganizerName => Organizer.ToString();

        [JsonIgnore]
        public bool IsSerial => Type == OData.Exception || Type == OData.Occurrence;

        [JsonIgnore]
        public bool IsSingle => !IsSerial;

        [JsonIgnore]
        public int Index { get; set; }

        public Location Location { get; set; }

        public List<Attendee> Attendees { get; set; }

        public class EventRecurrence
        {
            public Pattern Pattern { get; set; }
            public Range Range { get; set; }
        }

        public class Pattern
        {
            public string Type { get; set; }
            public int Interval { get; set; }
            public int DayOfMonth { get; set; }
            public int Month { get; set; }
            public List<string> DaysOfWeek { get; set; }
            public string FirstDayOfWeek { get; set; }
            public string Index { get; set; }
        }

        public class Range
        {
            public string Type { get; set; }

            public string StartDate { get; set; }
            public string EndDate { get; set; }

            public int NumberOfOccurrences { get; set; }
        }
    }

    public class Body
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public class Organizer
    {
        public EmailAddress EmailAddress { get; set; }

        public override string ToString()
        {
            return EmailAddress.ToString();
        }
    }

    public class ZonedDateTime
    {
        public DateTime DateTime { get; set; }
        public string TimeZone { get; set; }
    }
}
