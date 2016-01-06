package com.microsoft.office365.meetingmgr;

import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidate;

import java.text.DateFormat;
import java.text.ParseException;
import java.util.Date;
import java.util.List;
import java.util.TimeZone;

/**
 * List adapter for free time slots
 */
public class TimeslotAdapter extends BaseListAdapter<MeetingTimeCandidate> {

    public TimeslotAdapter(ListView view, List<MeetingTimeCandidate> objects) {
        super(view, objects, R.layout.timeslot_row);
    }

    @Override
    protected void setView(MeetingTimeCandidate timeCandidate) {
        DateFormat inFormat = DateFmt.instance("HH:mm:ss.SSS");
        String timeZone = timeCandidate.MeetingTimeSlot.Start.TimeZone;

        if (timeZone.toLowerCase().contains("microsoft/utc")) {
            // Some of the slots can be in UTC
            inFormat.setTimeZone(TimeZone.getTimeZone("UTC"));
            timeZone = TimeZone.getDefault().getDisplayName();
        }

        Date start;
        Date end;

        try {
            start = inFormat.parse(timeCandidate.MeetingTimeSlot.Start.Time);
            end = inFormat.parse(timeCandidate.MeetingTimeSlot.End.Time);
        } catch (ParseException e) {
            ErrorLogger.log(e);
            return;
        }

        // In case the slot is in UTC, convert it to local time zone
        {
            inFormat.setTimeZone(TimeZone.getDefault());
            timeCandidate.MeetingTimeSlot.Start.Time = inFormat.format(start);
            timeCandidate.MeetingTimeSlot.End.Time = inFormat.format(end);
        }

        DateFormat outFormat = DateFmt.instance("HH:mm");

        String startString = outFormat.format(start);
        String endString = outFormat.format(end);

        setText(R.id.timeSlot, String.format("%s - %s", startString, endString));
        setText(R.id.timeZone, timeZone);
    }
}
