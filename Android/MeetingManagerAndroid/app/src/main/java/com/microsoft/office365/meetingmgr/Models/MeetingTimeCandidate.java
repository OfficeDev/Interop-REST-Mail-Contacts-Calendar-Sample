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
