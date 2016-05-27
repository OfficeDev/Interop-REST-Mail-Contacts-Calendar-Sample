/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.RadioButton;
import android.widget.Spinner;
import android.widget.TextView;

import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.List;

/**
 * Handles modification of recurrent event fields
 */
public class RecurrenceActivity extends BaseActivity {
    private Spinner mSpnMode;
    private Spinner mSpnFirstLast;
    private Spinner mSpnFirstLast2;
    private Spinner mSpnDayOfWeek;
    private Spinner mSpnDayOfWeek2;
    private Spinner mSpnMonth;
    private Spinner mSpnMonth2;
    private OccurrenceType mType;
    private FlexRadioGroup mDailyGroup;
    private FlexRadioGroup mMonthlyGroup;
    private FlexRadioGroup mYearlyGroup;
    private FlexRadioGroup mRangeGroup;
    private Button mBtnDone;
    private TextView mTextStartDate;
    private TextView mTextEndDate;
    private RadioButton mRadEndBy;

    private View mDailyLayout;
    private View mWeeklyLayout;
    private View mMonthlyLayout;
    private View mYearlyLayout;

    private CheckList mWeekDayChecks;

    private Calendar mStartDate = Calendar.getInstance();
    private Calendar mEndDate = Calendar.getInstance();

    private final static List<String> INDICES = Arrays.asList(OData.FIRST, OData.SECOND, OData.THIRD, OData.FOURTH, OData.LAST);

    private Meeting.Recurrence mRecurrence;
    private Meeting mMeeting;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_recurrence);

        mMeeting = getArg(Meeting.getClassDeducer());
        mRecurrence = mMeeting.Recurrence;
        boolean isNew = mRecurrence == null;

        if (isNew) {
            mRecurrence = new Meeting.Recurrence();
        }

        mWeekDayChecks = new CheckList(this,
                R.id.monday, R.id.tuesday, R.id.wednesday,
                R.id.thursday, R.id.friday, R.id.saturday, R.id.sunday);

        initViews();
        setRecurrenceDefaults(mRecurrence);
        populateViews(isNew);
        setEventListeners();
    }

    private void populateViews(boolean isNew) {
        bindSpinners();

        mRangeGroup = new FlexRadioGroup(this, R.id.no_end, R.id.end_by, R.id.end_after);
        mDailyGroup = new FlexRadioGroup(this, R.id.every, R.id.every_weekday);
        mMonthlyGroup = new FlexRadioGroup(this, R.id.month_day, R.id.week_day);
        mYearlyGroup = new FlexRadioGroup(this, R.id.absolute_day, R.id.relative_day);

        mStartDate.setTime(DateFmt.dateFromApiDateString(mRecurrence.Range.StartDate));
        mEndDate.setTime(DateFmt.dateFromApiDateString(mRecurrence.Range.EndDate));

        bindDateField(mTextStartDate, mStartDate);
        bindDateField(mTextEndDate, mEndDate);

        switch (mRecurrence.Range.Type.toLowerCase()) {
            case OData.NO_END:
                mRangeGroup.setChecked(R.id.no_end);
                break;
            case OData.END_BY:
                mRangeGroup.setChecked(R.id.end_by);
                break;
            case OData.END_AFTER:
                mRangeGroup.setChecked(R.id.end_after);
                break;
        }

        populatePatternViews();

        setTextInteger(R.id.number, mRecurrence.Range.NumberOfOccurrences);

        mTextEndDate.setEnabled(mRadEndBy.isChecked());

        mSpnMode.setEnabled(isNew);
        mSpnMode.setSelection(mType.getValue(), true);

        setVisiblePatternView();
    }

    private void bindSpinners() {
        bindSimpleSpinner(mSpnFirstLast, R.array.first_last);
        bindSimpleSpinner(mSpnFirstLast2, R.array.first_last);

        List<String> daysOfWeekItems = DateFmt.getLocalWeekDays();
        List<String> monthItems = DateFmt.getLocalMonths();

        bindSimpleSpinner(mSpnDayOfWeek, daysOfWeekItems);
        bindSimpleSpinner(mSpnDayOfWeek2, daysOfWeekItems);

        bindSimpleSpinner(mSpnMonth, monthItems);
        bindSimpleSpinner(mSpnMonth2, monthItems);

        ArrayAdapter adapter = ArrayAdapter.createFromResource(this, R.array.daily_weekly, R.layout.mode_spinner_item);
        adapter.setDropDownViewResource(R.layout.mode_spinner_dropdown_item);
        mSpnMode.setAdapter(adapter);
        mSpnMode.setSelection(0);    // default selection
    }

    private void setEventListeners() {
        for (CheckBox cb : mWeekDayChecks) {
            cb.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    setOkEnabled(mType == OccurrenceType.Weekly && mWeekDayChecks.isAnyChecked());
                }
            });

            mRadEndBy.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    mTextEndDate.setEnabled(isChecked);
                }
            });
        }

        mBtnDone.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                try {
                    finish(buildRecurrence());
                } catch (Exception e) {
                    showAlert(e.getMessage());
                }
            }
        });

        mSpnMode.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                mType = OccurrenceType.values()[position];

                if (mType == OccurrenceType.Weekly) {
                    setOkEnabled(mWeekDayChecks.isAnyChecked());
                }

                setVisiblePatternView();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {
            }
        });
    }

    private void populatePatternViews() {
        switch (mRecurrence.Pattern.Type.toLowerCase()) {
            case OData.DAILY:
                mType = OccurrenceType.Daily;
                setTextInteger(R.id.interval_day, mRecurrence.Pattern.Interval);
                break;
            case OData.WEEKLY:
                mType = OccurrenceType.Weekly;
                setTextInteger(R.id.interval_week, mRecurrence.Pattern.Interval);

                for (String day : mRecurrence.Pattern.DaysOfWeek) {
                    mWeekDayChecks.findByTag(day).setChecked(true);
                }
                break;
            case OData.RELATIVE_MONTHLY:
            case OData.ABSOLUTE_MONTHLY:
                mMonthlyGroup.setChecked(
                        mRecurrence.Pattern.Type.equalsIgnoreCase(OData.RELATIVE_MONTHLY) ?
                                R.id.week_day : R.id.month_day);
                mType = OccurrenceType.Monthly;
                setTextInteger(R.id.interval_month, mRecurrence.Pattern.Interval);
                setTextInteger(R.id.interval_month2, mRecurrence.Pattern.Interval);
                setTextInteger(R.id.day_of_month, mRecurrence.Pattern.DayOfMonth);
                setSpinnerInteger(R.id.first_last, INDICES.indexOf(mRecurrence.Pattern.Index.toLowerCase()));

                if (mRecurrence.Pattern.DaysOfWeek.size() == 1) {
                    String day = mRecurrence.Pattern.DaysOfWeek.get(0);
                    int index = DateFmt.getDayOfWeekIndex(day);
                    setSpinnerInteger(R.id.day_of_week, index);
                }
                break;
            case OData.RELATIVE_YEARLY:
            case OData.ABSOLUTE_YEARLY:
                mYearlyGroup.setChecked(
                        mRecurrence.Pattern.Type.equalsIgnoreCase(OData.RELATIVE_YEARLY) ?
                                R.id.relative_day : R.id.absolute_day);
                mType = OccurrenceType.Yearly;
                setTextInteger(R.id.interval_year, mRecurrence.Pattern.Interval);
                setTextInteger(R.id.day_of_month2, mRecurrence.Pattern.DayOfMonth);
                setSpinnerInteger(R.id.month, DateFmt.apiMonthToIndex(mRecurrence.Pattern.Month));
                setSpinnerInteger(R.id.month2, DateFmt.apiMonthToIndex(mRecurrence.Pattern.Month));

                if (mRecurrence.Pattern.DaysOfWeek.size() == 1) {
                    String day = mRecurrence.Pattern.DaysOfWeek.get(0);
                    int index = DateFmt.getDayOfWeekIndex(day);
                    setSpinnerInteger(R.id.day_of_week2, index);
                }
                break;
        }
    }

    private void setRecurrenceDefaults(Meeting.Recurrence recurrence) {
        Calendar current = Calendar.getInstance();

        if (recurrence.Range.StartDate == null) {
            recurrence.Range.StartDate = DateFmt.toApiUtcString(current.getTime());
        }

        if (!recurrence.Range.Type.equalsIgnoreCase(OData.END_BY)) {
            current.set(Calendar.MONTH, Calendar.DECEMBER);
            current.set(Calendar.DAY_OF_MONTH, 31);
            recurrence.Range.EndDate = DateFmt.toApiUtcString(current.getTime());
        }
    }

    private void initViews() {
        mBtnDone = (Button) findViewById(R.id.done);
        mSpnMode = (Spinner) findViewById(R.id.mode);
        mSpnDayOfWeek = (Spinner) findViewById(R.id.day_of_week);
        mSpnDayOfWeek2 = (Spinner) findViewById(R.id.day_of_week2);
        mSpnFirstLast = (Spinner) findViewById(R.id.first_last);
        mSpnFirstLast2 = (Spinner) findViewById(R.id.first_last2);
        mSpnMonth = (Spinner) findViewById(R.id.month);
        mSpnMonth2 = (Spinner) findViewById(R.id.month2);

        mTextStartDate = (TextView) findViewById(R.id.start_date);
        mTextEndDate = (TextView) findViewById(R.id.end_date);

        mDailyLayout = findViewById(R.id.daily);
        mWeeklyLayout = findViewById(R.id.weekly);
        mMonthlyLayout = findViewById(R.id.monthly);
        mYearlyLayout = findViewById(R.id.yearly);

        mRadEndBy = (RadioButton) findViewById(R.id.end_by);
    }

    private Meeting.Recurrence buildRecurrence() throws Exception {
        Meeting.Recurrence recurrence = new Meeting.Recurrence();

        switch (mType) {
            case Daily:
                if (mDailyGroup.getCheckedRadioButtonId() == R.id.every_weekday) {
                    recurrence.Pattern.Type = OData.WEEKLY;
                    recurrence.Pattern.Interval = 1;
                    recurrence.Pattern.Index = OData.FIRST;
                    addAllWeekdays(recurrence);
                } else {
                    recurrence.Pattern.Type = OData.DAILY;
                    recurrence.Pattern.Interval = getTextInteger(R.id.interval_day, 1);
                }
                break;

            case Weekly:
                recurrence.Pattern.Type = OData.WEEKLY;
                recurrence.Pattern.Interval = getTextInteger(R.id.interval_week, 1);

                for (CheckBox cb : mWeekDayChecks) {
                    if (cb.isChecked()) {
                        recurrence.Pattern.DaysOfWeek.add(cb.getTag().toString());
                    }
                }
                break;

            case Monthly:
                if (mMonthlyGroup.getCheckedRadioButtonId() == R.id.month_day) {
                    recurrence.Pattern.Type = OData.ABSOLUTE_MONTHLY;
                    recurrence.Pattern.DayOfMonth = getTextInteger(R.id.day_of_month, 1, 31);
                    recurrence.Pattern.Interval = getTextInteger(R.id.interval_month, 1);
                } else {
                    recurrence.Pattern.Type = OData.RELATIVE_MONTHLY;
                    recurrence.Pattern.Index = INDICES.get(getSpinnerInteger(R.id.first_last));
                    String apiDayOfWeek = DateFmt.indexToApiDayOfWeek(getSpinnerInteger(R.id.day_of_week));
                    recurrence.Pattern.DaysOfWeek.add(apiDayOfWeek);
                    recurrence.Pattern.Interval = getTextInteger(R.id.interval_month2, 1);
                }
                break;

            case Yearly:
                recurrence.Pattern.Interval = getTextInteger(R.id.interval_year, 1);

                if (mYearlyGroup.getCheckedRadioButtonId() == R.id.absolute_day) {
                    recurrence.Pattern.Type = OData.ABSOLUTE_YEARLY;
                    recurrence.Pattern.Month = DateFmt.indexToApiMonth(getSpinnerInteger(R.id.month));
                    recurrence.Pattern.DayOfMonth = getTextInteger(R.id.day_of_month2, 1, 31);
                } else {
                    recurrence.Pattern.Type = OData.RELATIVE_YEARLY;
                    recurrence.Pattern.Index = INDICES.get(getSpinnerInteger(R.id.first_last2));
                    String apiDayOfWeek = DateFmt.indexToApiDayOfWeek(getSpinnerInteger(R.id.day_of_week2));
                    recurrence.Pattern.DaysOfWeek.add(apiDayOfWeek);
                    recurrence.Pattern.Month = DateFmt.indexToApiMonth(getSpinnerInteger(R.id.month2));
                }
                break;
        }

        recurrence.Range.StartDate = mMeeting.formatRecurrenceStartDate(mStartDate.getTime());

        switch (mRangeGroup.getCheckedRadioButtonId()) {
            case R.id.no_end:
                recurrence.Range.Type = OData.NO_END;
                break;
            case R.id.end_after:
                recurrence.Range.Type = OData.END_AFTER;
                recurrence.Range.NumberOfOccurrences = getTextInteger(R.id.number, 1);
                break;
            case R.id.end_by:
                recurrence.Range.Type = OData.END_BY;
                recurrence.Range.EndDate = DateFmt.toApiUtcString(mEndDate.getTime());
                break;
        }

        return recurrence;
    }

    private void setTextInteger(int id, int value) {
        findTextView(id).setText(String.valueOf(value));
    }

    private int getTextInteger(int id) {
        String interval = findTextView(id).getText().toString();
        return Integer.valueOf(interval);
    }

    private int getTextInteger(int id, int minValue) throws Exception {
        int value = getTextInteger(id);

        if (value < minValue) {
            throw new Exception(String.format("Value '%s' is invalid. Must be greater or equal to %s", value, minValue));
        }
        return value;
    }

    private int getTextInteger(int id, int minValue, int maxValue) throws Exception {
        int value = getTextInteger(id);

        if (value < minValue || value > maxValue) {
            throw new Exception(String.format("Value '%s' is invalid. Must be between %s and %s", value, minValue, maxValue));
        }
        return value;
    }

    private int getSpinnerInteger(int id) {
        return findSpinner(id).getSelectedItemPosition();
    }

    private void setSpinnerInteger(int id, int value) {
        findSpinner(id).setSelection(value);
    }

    private Spinner findSpinner(int id) {
        return (Spinner) findViewById(id);
    }

    private TextView findTextView(int id) {
        return (TextView) findViewById(id);
    }

    private void addAllWeekdays(Meeting.Recurrence recurrence) {
        recurrence.Pattern.DaysOfWeek.add(OData.MONDAY);
        recurrence.Pattern.DaysOfWeek.add(OData.TUESDAY);
        recurrence.Pattern.DaysOfWeek.add(OData.WEDNESDAY);
        recurrence.Pattern.DaysOfWeek.add(OData.THURSDAY);
        recurrence.Pattern.DaysOfWeek.add(OData.FRIDAY);
    }

    private void setVisiblePatternView() {
        mDailyLayout.setVisibility(View.GONE);
        mWeeklyLayout.setVisibility(View.GONE);
        mMonthlyLayout.setVisibility(View.GONE);
        mYearlyLayout.setVisibility(View.GONE);

        switch (mType) {
            case Daily:
                mDailyLayout.setVisibility(View.VISIBLE);
                setOkEnabled(true);
                break;
            case Weekly:
                mWeeklyLayout.setVisibility(View.VISIBLE);
                break;
            case Monthly:
                mMonthlyLayout.setVisibility(View.VISIBLE);
                setOkEnabled(true);
                break;
            case Yearly:
                mYearlyLayout.setVisibility(View.VISIBLE);
                setOkEnabled(true);
                break;
        }
    }

    private void setOkEnabled(boolean enabled) {
        mBtnDone.setEnabled(enabled);
    }

    private static class CheckList extends ArrayList<CheckBox> {

        CheckList(Activity activity, int... ids) {
            for (int id : ids) {
                add((CheckBox) activity.findViewById(id));
            }
        }

        boolean isAnyChecked() {
            for (CheckBox cb : this) {
                if (cb.isChecked()) {
                    return true;
                }
            }
            return false;
        }

        CheckBox findByTag(String tag) {
            for (CheckBox cb : this) {
                if (tag.equalsIgnoreCase(cb.getTag().toString())) {
                    return cb;
                }
            }
            return null;
        }
    }

    /**
     * Note that we do use the value as an index in UI
     */
    private enum OccurrenceType {
        Daily(0),
        Weekly(1),
        Monthly(2),
        Yearly(3);

        private final int value;
        OccurrenceType(int value) {
            this.value = value;
        }

        public int getValue() {
            return value;
        }
    }
}
