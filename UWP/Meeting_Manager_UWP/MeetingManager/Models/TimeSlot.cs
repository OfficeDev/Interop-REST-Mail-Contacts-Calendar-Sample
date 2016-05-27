//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;

namespace MeetingManager.Models
{
    public class TimeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public static TimeSlot Parse(MeetingTimeCandidate mtc)
        {
            var slot = mtc.MeetingTimeSlot;

            var start = DateTime.Parse(slot.Start.Date);
            var startTime = DateTime.Parse(slot.Start.Time);
            start += startTime.TimeOfDay;

            var end = DateTime.Parse(slot.End.Date);
            var endTime = DateTime.Parse(slot.End.Time);
            end += endTime.TimeOfDay;

            if (slot.Start.TimeZone.ToUpper().Contains("UTC"))
            {
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

                start = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.Utc, TimeZoneInfo.Local);
                end = TimeZoneInfo.ConvertTime(end, TimeZoneInfo.Utc, TimeZoneInfo.Local);
            }

            return new TimeSlot
            {
                Start = start,
                End = end
            };
        }
    }
}
