/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.app.DatePickerDialog;
import android.app.Dialog;
import android.app.DialogFragment;
import android.app.FragmentManager;
import android.os.Bundle;
import android.view.View;
import android.widget.DatePicker;
import android.widget.TextView;

import java.util.Calendar;

/**
 * Binds text field to date picker dialog
 */
public class DateSelector {
    public DateSelector(final FragmentManager fragmentManager, final TextView textDate, final Calendar calendar, final Runnable callback) {
        setText(textDate, calendar);

        final DatePickerDialog.OnDateSetListener listener = new DatePickerDialog.OnDateSetListener() {
            @Override
            public void onDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth) {
                calendar.set(year, monthOfYear, dayOfMonth);

                if (callback != null) {
                    callback.run();
                }
                setText(textDate, calendar);
            }
        };

        textDate.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                new DialogFragment() {
                    @Override
                    public Dialog onCreateDialog(Bundle savedInstanceState) {
                        // Use the current date as the default date in the picker
                        int year = calendar.get(Calendar.YEAR);
                        int month = calendar.get(Calendar.MONTH);
                        int day = calendar.get(Calendar.DAY_OF_MONTH);

                        return new DatePickerDialog(getActivity(), listener, year, month, day);
                    }
                }.show(fragmentManager, null);
            }
        });
    }

    private static void setText(TextView textView, Calendar calendar) {
        String date = DateFmt.toDateWithDayOfWeekString(calendar.getTime());
        textView.setText(date);
    }
}
