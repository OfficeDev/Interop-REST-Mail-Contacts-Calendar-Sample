package com.microsoft.office365.meetingmgr;

import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.TimeZone;

/**
 * Meetings list adapter
 */
public class MeetingsAdapter extends BaseListAdapter<Meeting> {
    public MeetingsAdapter(ListView view, List<Meeting> data) {
        super(view, data, R.layout.meeting_row);
        view.setAdapter(this);
    }

    @Override
    public void setView(Meeting mtg) {
        if (mtg.IsAllDay) {
            String allDay = String.format("All day %s", DateFmt.toShortDateString(mtg.getStart()));
            setText(R.id.startTime, allDay);
        } else {
            setText(R.id.startTime, DateFmt.toTimeString(mtg.getStart()));
        }
        setText(R.id.subject, mtg.Subject);
        setText(R.id.location, mtg.Location.DisplayName);
        setText(R.id.organizer, mtg.Organizer.toString());
    }
}
