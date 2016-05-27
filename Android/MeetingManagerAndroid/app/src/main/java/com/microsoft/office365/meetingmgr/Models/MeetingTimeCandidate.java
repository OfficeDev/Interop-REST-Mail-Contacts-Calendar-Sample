/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of a single meeting time candidate
 */
public class MeetingTimeCandidate {
    public MeetingTimeSlot MeetingTimeSlot;
    public String Confidence;
    public String OrganizerAvailability;
    public AttendeeAvailability[]  AttendeeAvailability;

    public static class AttendeeAvailability {
        Attendee Attendee;
        String Availability;
    }

    public static class Attendee {
        EmailAddress EmailAddress;
        String Type;
    }
}
