/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.app.Dialog;
import android.app.DialogFragment;
import android.app.FragmentManager;
import android.app.TimePickerDialog;
import android.content.Context;
import android.os.Bundle;
import android.text.format.DateFormat;
import android.view.View;
import android.widget.TextView;
import android.widget.TimePicker;

import java.util.Calendar;

/**
 * Binds text field to time picker dialog
 */
public class TimeSelector {
    public TimeSelector(final FragmentManager fragmentManager, final TextView textView, final Calendar calendar, final Runnable callback) {
        textView.setText(DateFmt.toTimeString(calendar.getTime()));

        final TimePickerDialog.OnTimeSetListener listener = new TimePickerDialog.OnTimeSetListener() {
            @Override
            public void onTimeSet(TimePicker view, int hourOfDay, int minute) {
                calendar.set(Calendar.HOUR_OF_DAY, hourOfDay);
                calendar.set(Calendar.MINUTE, minute);

                if (callback != null) {
                    callback.run();
                }
                textView.setText(DateFmt.toTimeString(calendar.getTime()));
            }
        };

        textView.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                new DialogFragment() {
                    @Override
                    public Dialog onCreateDialog(Bundle savedInstanceState) {
                        return createTimePickerDialog(getActivity(), calendar, listener);
                    }
                }.show(fragmentManager, null);
            }
        });
    }

    private static Dialog createTimePickerDialog(Context context, Calendar calendar, TimePickerDialog.OnTimeSetListener listener) {
        int hour = calendar.get(Calendar.HOUR_OF_DAY);
        int minute = calendar.get(Calendar.MINUTE);

        boolean is24 = DateFormat.is24HourFormat(context);

        return new TimePickerDialog(context, listener, hour, minute, is24);
    }
}
