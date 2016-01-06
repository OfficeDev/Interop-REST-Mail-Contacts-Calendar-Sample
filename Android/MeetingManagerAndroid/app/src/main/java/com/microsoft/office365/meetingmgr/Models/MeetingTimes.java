package com.microsoft.office365.meetingmgr.Models;

import java.util.ArrayList;
import java.util.List;

/**
 * Representation of request body for meeting time candidates
 */
public class MeetingTimes {
    public List<AttendeeBase> Attendees = new ArrayList<>();
    public TimeConstraint TimeConstraint = new TimeConstraint();
    public LocationConstraint LocationConstraint = new LocationConstraint();
    public String MeetingDuration = "PT30M";

    public static class TimeConstraint {
        public List<MeetingTimeSlot> Timeslots = new ArrayList<>();
    }

    public static class LocationConstraint {
        public boolean IsRequired;
        public boolean SuggestLocation;
        public List<Location> Locations = new ArrayList<>();
    }

    public static class AttendeeBase {
        public EmailAddress EmailAddress = new EmailAddress();

        @Override
        public String toString() {
            return EmailAddress.Address;
        }
    }
}
