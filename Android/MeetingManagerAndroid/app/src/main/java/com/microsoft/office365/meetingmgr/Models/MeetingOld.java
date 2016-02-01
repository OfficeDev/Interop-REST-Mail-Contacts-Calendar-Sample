package com.microsoft.office365.meetingmgr.Models;
import com.microsoft.office365.meetingmgr.DateFmt;

import java.util.Date;
import java.util.TimeZone;

/**
 * Meeting (event) representation in Office365 API
 */
public class MeetingOld extends Meeting {
    public String Start;
    public String End;

    public String StartTimeZone = TimeZone.getDefault().getDisplayName();
    public String EndTimeZone = TimeZone.getDefault().getDisplayName();

    public String DateTimeCreated;

    @Override
    public void setStart(String time) {
        Start = time;
    }

    @Override
    public String getStart() {
        return getTimeConditional(Start);
    }

    @Override
    public void setEnd(String time) {
        End = time;
    }

    @Override
    public String getEnd() {
        return getTimeConditional(End);
    }

    private String getTimeConditional(String timeString) {
        if (IsAllDay) {
            return DateFmt.getApiDateTime(timeString, "UTC");
        } else {
            return timeString;
        }
    }

    @Override
    public void setStartTimeZone(String timeZone) {
        StartTimeZone = timeZone;
    }

    @Override
    public String getStartTimeZone() {
        return StartTimeZone;
    }

    @Override
    public void setEndTimeZone(String timeZone) {
        EndTimeZone = timeZone;
    }

    @Override
    public String getEndTimeZone() {
        return EndTimeZone;
    }

    @Override
    public String formatRecurrenceStartDate(Date startDate) {
        return DateFmt.toApiUtcString(startDate);
    }

    @Override
    public String getCreatedTime() {
        return DateTimeCreated;
    }

    @Override
    public String getCreatedTimePropertyName() {
        return "DateTimeCreated";
    }
}
