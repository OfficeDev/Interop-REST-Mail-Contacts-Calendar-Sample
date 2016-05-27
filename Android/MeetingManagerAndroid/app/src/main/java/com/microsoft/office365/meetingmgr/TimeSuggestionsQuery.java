/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import com.microsoft.office365.meetingmgr.Models.Attendee;
import com.microsoft.office365.meetingmgr.Models.Location;
import com.microsoft.office365.meetingmgr.Models.Meeting;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidate;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidates;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeSlot;
import com.microsoft.office365.meetingmgr.Models.MeetingTimes;
import com.microsoft.office365.meetingmgr.Models.User;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

/**
 * Handles OData query to get free time slots
 */
public class TimeSuggestionsQuery {
    private final Meeting mMeeting;

    public TimeSuggestionsQuery(Meeting meeting) {
        mMeeting = meeting;
    }

    public MeetingTimeCandidates getTimeCandidates(HttpHelper hp) {
        List<MeetingTimeCandidate> candidates = new ArrayList<>();

        candidates.addAll(getCandidates(hp, "8:00:00", "11:00:00"));
        candidates.addAll(getCandidates(hp, "11:00:00", "15:00:00"));
        candidates.addAll(getCandidates(hp, "15:00:00", "18:00:00"));

        return new MeetingTimeCandidates(candidates);
    }

    private MeetingTimes buildRequest(String startTime, String endTime) {
        MeetingTimes times = new MeetingTimes();
        User user = Manager.Instance.getUser();     // app user/organizer

        for (Attendee att : mMeeting.Attendees) {
            MeetingTimes.AttendeeBase attBase = new MeetingTimes.AttendeeBase();

            // Exclude organizer from attendee list
            if (!att.EmailAddress.Address.equalsIgnoreCase(user.id)) {
                attBase.EmailAddress.Address = att.EmailAddress.Address;
                times.Attendees.add(attBase);
            }
        }

        MeetingTimeSlot timeSlot = new MeetingTimeSlot();

        Date date = DateFmt.dateFromApiDateString(mMeeting.getStart());

        String dateString = DateFmt.toApiDateString(date);

        timeSlot.Start = new MeetingTimeSlot.Time(dateString, startTime, mMeeting.getStartTimeZone());
        timeSlot.End = new MeetingTimeSlot.Time(dateString, endTime, mMeeting.getEndTimeZone());

        times.TimeConstraint.Timeslots.add(timeSlot);

        if (!Utils.isNullOrEmpty(mMeeting.Location.DisplayName)) {
            times.LocationConstraint.Locations.add(new Location(mMeeting.Location.DisplayName));
        }

        return times;
    }

    private List<MeetingTimeCandidate> getCandidates(HttpHelper hp, String startTime, String endTime) {
        MeetingTimes times = buildRequest(startTime, endTime);
        String uri = "https://outlook.office365.com/api/beta/me/findmeetingtimes";

        MeetingTimeCandidates result = hp.postItem(uri, times, MeetingTimeCandidates.class);

        return result.value;
    }

}
