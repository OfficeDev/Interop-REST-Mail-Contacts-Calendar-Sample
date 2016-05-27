/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of a single meeting time slot
 */
public class MeetingTimeSlot {
    public Time Start;
    public Time End;

    public static class Time {
        public String Date;
        public String Time;
        public String TimeZone;

        private Time() {}

        public Time(String date, String time, String timeZone) {
            Date = date;
            Time = time;
            TimeZone = timeZone;
        }
    }
}
