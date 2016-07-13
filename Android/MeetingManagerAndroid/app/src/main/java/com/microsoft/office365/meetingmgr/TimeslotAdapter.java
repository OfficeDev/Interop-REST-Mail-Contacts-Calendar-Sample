/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
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
        Date start;
        Date end;

        try {
            start = inFormat.parse(timeCandidate.MeetingTimeSlot.Start.Time);
            end = inFormat.parse(timeCandidate.MeetingTimeSlot.End.Time);
        } catch (ParseException e) {
            ErrorLogger.log(e);
            return;
        }

        // Assuming time slots are always in UTC
        start = DateFmt.utcToLocal(start);
        end = DateFmt.utcToLocal(end);

        DateFormat outFormat = DateFmt.instance("HH:mm");

        String startString = outFormat.format(start);
        String endString = outFormat.format(end);

        setText(R.id.timeSlot, String.format("%s - %s", startString, endString));
    }
}
