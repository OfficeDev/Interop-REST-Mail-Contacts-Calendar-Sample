/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.database.DataSetObserver;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;

import com.microsoft.office365.meetingmgr.Models.Attendee;
import com.microsoft.office365.meetingmgr.Models.Meeting;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidate;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidates;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeSlot;
import com.microsoft.office365.meetingmgr.Models.User;

import java.text.DateFormat;
import java.text.ParseException;
import java.util.Calendar;
import java.util.Date;
import java.util.TimeZone;
import java.util.concurrent.TimeUnit;

/**
 * Handles meeting creation and modification
 *
 * Uses the following REST APIs:
 * 1. Create event
 * 2. Update event
 * 3. Find meeting times
 */
public class NewMeetingActivity extends BaseActivity {
    private EditText mEditTitle;
    private TextView mTextDate;
    private TextView mRecurrentDate;
    private TextView mTextStartTime;
    private TextView mTextEndTime;

    private EditText mEditLocation;
    private CheckBox mChkAllDay;

    private EditText mEditBody;
    private ListView mListAttendees;

    private Button mBtnDone;
    private Button mBtnAddAttendee;

    private Calendar mStartCal = Calendar.getInstance();
    private Calendar mEndCal = Calendar.getInstance();

    private Meeting mMeeting;

    private Meeting.Recurrence mRecurrence;
    private String mMeetingId;

    private User mUser;     // app user/organizer
    private AttendeesAdapter mAttAdapter;
    private LoaderHolder mCreateEvent;
    private LoaderHolder mAsapLoader;

    private ActivityHolder mRecurrenceActivity;
    private ActivityHolder mLocationActivity;
    private ActivityHolder mAttendeeActivity;
    private ActivityHolder mContactsActivity;
    private ActivityHolder mTimeSuggestionActivity;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_new_meeting);
        mUser = Manager.Instance.getUser();

        mMeeting = getArg(Meeting.getClassDeducer());

        if (mMeeting == null) {
            setTitle(getString(R.string.title_activity_create_event));
            mMeeting = createNewMeeting();
        } else {
            setTitle(getString(R.string.title_activity_modify_event));
            mStartCal.setTime(DateFmt.dateFromApiDateString(mMeeting.getStart()));
            mEndCal.setTime(DateFmt.dateFromApiDateString(mMeeting.getEnd()));
        }

        mRecurrence = mMeeting.Recurrence;
        mMeetingId = mMeeting.Id;

        initViews();
        bindDateTime();
        bindMenus();

        populateControls();
        setEventListeners();

        registerLoaders();
        registerActivities();
    }

    private void bindMenus() {
        bindContextMenu(mListAttendees, R.menu.attendee_context_menu, new ContextMenuHandler<Attendee>() {
            @Override
            public void onCreate(Attendee object) {
                findItem(R.id.delete).setEnabled(!isOrganizer(object));
            }

            @Override
            public boolean onItemSelected(int itemId, Attendee object) {
                switch (itemId) {
                    case R.id.delete:
                        mAttAdapter.remove(object);
                        return true;
                }
                return false;
            }
        });

        bindPopupMenu(mBtnAddAttendee, R.menu.attendee_menu, new PopupMenuHandler() {
            @Override
            public boolean onItemSelected(int itemId) {
                switch (itemId) {
                    case R.id.gal:
                        mAttendeeActivity.start("true");
                        return true;
                    case R.id.contacts:
                        mContactsActivity.start();
                        return true;
                }
                return false;
            }
        });
    }

    private void setEventListeners() {
        mEditTitle.addTextChangedListener(new LocalTextWatcher(mEditTitle));

        mChkAllDay.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {

                if (!mMeeting.IsAllDay && isChecked) {
                    mStartCal.set(Calendar.HOUR_OF_DAY, 0);
                    mEndCal = (Calendar) mStartCal.clone();
                    mEndCal.add(Calendar.HOUR_OF_DAY, 24);
                } else {
                    setDefaultTimes();
                }

                bindTimeFields();
                mMeeting.IsAllDay = isChecked;
                setTimeEnabled();
            }
        });
    }

    private void setTimeEnabled() {
        mTextStartTime.setEnabled(!mMeeting.IsAllDay);
        mTextEndTime.setEnabled(!mMeeting.IsAllDay);
    }

    private void registerLoaders() {
        mCreateEvent = registerLoadHandler(new CreateEventLoader());

        mAsapLoader = registerLoadHandler(new LoadHandler<MeetingTimeCandidates>() {
            @Override
            HttpCallable onCreate() {
                final Meeting meeting = createMeetingFromView();

                return new HttpCallable() {
                    @Override
                    public MeetingTimeCandidates call(HttpHelper hp) throws Exception {
                        TimeSuggestionsQuery tsq = new TimeSuggestionsQuery(meeting);
                        return tsq.getTimeCandidates(hp);
                    }
                };
            }

            @Override
            void onFinished(MeetingTimeCandidates data) {
                MeetingTimeCandidate slot = selectEarliest(data);

                if (slot != null) {
                    populateTimeFromSlot(slot);
                }
            }
        });
    }

    private class CreateEventLoader<T extends Meeting>  extends LoadHandler<T> {
        @Override
        HttpCallable onCreate() {
            return new HttpCallable() {
                @Override
                public T call(HttpHelper hp) throws Exception {
                    @SuppressWarnings("unchecked")
                    T meeting = (T) createMeetingFromView();
                    String uri = "events/";

                    if (meeting.Id == null) {
                        return hp.postItem(uri, meeting);
                    } else {
                        uri += meeting.Id;
                        return hp.patchItem(uri, meeting);
                    }
                }
            };
        }

        @Override
        void onFinished(T data) {
            finish(data);
        }
    }

    private void registerActivities() {
        mRecurrenceActivity = registerActivity(RecurrenceActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                mRecurrence = getArg(Meeting.Recurrence.class);
                populateRecurrence();
            }
        });

        mLocationActivity = registerActivity(AddAttendeeActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                Attendee att = getArg(Attendee.class);
                mEditLocation.setText(att.EmailAddress.Name);
            }
        });

        mAttendeeActivity = registerActivity(AddAttendeeActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                mAttAdapter.add(getArg(Attendee.class));
            }
        });

        mContactsActivity = registerActivity(ContactsActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                mAttAdapter.add(getArg(Attendee.class));
            }
        });

        mTimeSuggestionActivity = registerActivity(TimeSuggestionsActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                MeetingTimeCandidate newTime = getArg(MeetingTimeCandidate.class);
                populateTimeFromSlot(newTime);
            }
        });
    }

    private void populateTimeFromSlot(MeetingTimeCandidate slot) {
        setTimeFromSlot(mStartCal, slot.MeetingTimeSlot.Start);
        setTimeFromSlot(mEndCal, slot.MeetingTimeSlot.End);

        bindTimeFields();
    }

    private void setTimeFromSlot(Calendar cal, MeetingTimeSlot.Time dateTime) {
        DateFormat inTimeFormat = DateFmt.instance("hh:mm:ss.SSS");
        DateFormat inDateFormat = DateFmt.instance("yyyy-MM-dd");

        try {
            Date time = inTimeFormat.parse(dateTime.Time);
            Date date = inDateFormat.parse(dateTime.Date);

            cal.setTime(time);
            setDateComponents(cal, date);
        } catch (ParseException e) {
            ErrorLogger.log(e);
        }
    }

    private void setDateComponents(Calendar cal, Date date) {
        Calendar dateCal = Calendar.getInstance();
        dateCal.setTime(date);

        copyCalendarDate(dateCal, cal);
    }

    private void bindDateTime() {
        bindDateField(mTextDate, mStartCal, new Runnable() {
            @Override
            public void run() {
                copyCalendarDate(mStartCal, mEndCal);
            }
        });

        bindTimeFields();
    }

    private void copyCalendarDate(Calendar calSrc, Calendar calDst) {
        calDst.set(calSrc.get(Calendar.YEAR),
                calSrc.get(Calendar.MONTH),
                calSrc.get(Calendar.DAY_OF_MONTH));
    }

    private void bindTimeFields() {
        bindTimeField(mTextStartTime, mStartCal, new Runnable() {
            @Override
            public void run() {
                if (!compareTimes(mStartCal, mEndCal)) {
                    mEndCal = (Calendar) mStartCal.clone();
                    mEndCal.add(Calendar.MINUTE, 30);

                    mTextEndTime.setText(DateFmt.toTimeString(mEndCal.getTime()));
                }
            }
        });

        bindTimeField(mTextEndTime, mEndCal, null);
    }

    private boolean compareTimes(Calendar start, Calendar end) {
        long startInMillis = start.getTimeInMillis();
        long endInMillis = end.getTimeInMillis();

        return (startInMillis < endInMillis &&
                TimeUnit.MILLISECONDS.toHours(endInMillis - startInMillis) < 24);
    }

    private boolean isOrganizer(Attendee item) {
        return item.EmailAddress.Address.equalsIgnoreCase(mUser.id);
    }

    public void onFindRoom(View v) {
        onSelectLocation(v);
    }

    public void onRecurrence(View v) {
        Meeting meeting = createMeetingFromView();
        meeting.Recurrence = mRecurrence;
        mRecurrenceActivity.start(mMeeting);
    }

    private void initViews() {
        mEditTitle = (EditText) findViewById(R.id.title);
        mTextDate = (TextView) findViewById(R.id.date);
        mRecurrentDate = (TextView) findViewById(R.id.recurrentDate);
        mTextStartTime = (TextView) findViewById(R.id.startTime);
        mTextEndTime = (TextView) findViewById(R.id.endTime);
        mEditLocation = (EditText) findViewById(R.id.location);
        mEditBody = (EditText) findViewById(R.id.body);
        mListAttendees = (ListView) findViewById(R.id.attendees);
        mBtnDone = (Button) findViewById(R.id.ok);
        mBtnAddAttendee = (Button) findViewById(R.id.addAttendee);
        mChkAllDay = (CheckBox) findViewById(R.id.all_day);
    }

    private class LocalTextWatcher extends MultiTextWatcher {

        private LocalTextWatcher(View view) {
           super(view);
        }

        @Override
        public void onTextChanged(CharSequence charSequence, int i, int i1, int i2) {
            mBtnDone.setEnabled(charSequence.length() > 0);
        }
    }

    public void onSelectLocation(View v) {
        mLocationActivity.start("false");
    }

    private Meeting createNewMeeting() {
        setDefaultTimes();

        Meeting meeting = Meeting.newInstance();
        meeting.setStart(DateFmt.toApiUtcString(mStartCal.getTime()));
        meeting.setEnd(DateFmt.toApiUtcString(mEndCal.getTime()));

        // We only set organizer when creating a new meeting and
        // don't care about organizer for existing ones
        meeting.Organizer.EmailAddress.Address = mUser.id;
        meeting.Organizer.EmailAddress.Name = mUser.DisplayName;

        // Always add organizer as an attendee
        meeting.Attendees.add(new Attendee(meeting.Organizer.EmailAddress));

        return meeting;
    }

    private void setDefaultTimes() {
        mStartCal = Calendar.getInstance();
        mStartCal.add(Calendar.MINUTE, 60);
        mStartCal.set(Calendar.MINUTE, (mStartCal.get(Calendar.MINUTE) / 60) * 60);
        mStartCal.set(Calendar.SECOND, 0);

        mEndCal.setTimeInMillis(mStartCal.getTimeInMillis() + TimeUnit.MINUTES.toMillis(30));
    }

    // Create meeting from the screen data
    private Meeting createMeetingFromView() {
        Meeting meeting = Meeting.newInstance();

        meeting.Id = mMeetingId;
        meeting.Subject = mEditTitle.getText().toString();
        meeting.IsAllDay = mChkAllDay.isChecked();

        if (HttpHelper.isUnified()) {
            meeting.setStart(DateFmt.toApiLocalString(mStartCal.getTime()));
            meeting.setEnd(DateFmt.toApiLocalString(mEndCal.getTime()));
        } else {
            meeting.setStart(DateFmt.toApiUtcString(mStartCal.getTime()));
            meeting.setEnd(DateFmt.toApiUtcString(mEndCal.getTime()));
        }
        meeting.Body.Content = mEditBody.getText().toString();

        meeting.Location.DisplayName = mEditLocation.getText().toString();

        meeting.Attendees = mAttAdapter.getList();

        // A server should take care of the Recurrence.Type field
        meeting.Recurrence = mRecurrence;

        return meeting;
    }

    private void populateControls() {
        mEditTitle.setText(mMeeting.Subject);
        mEditBody.setText(mMeeting.Body.getText());

        populateRecurrence();

        mEditLocation.setText(mMeeting.Location.DisplayName);

        mAttAdapter = mMeeting.bindAttendeeList(mListAttendees);
        mAttAdapter.registerDataSetObserver(new DataSetObserver() {
            @Override
            public void onChanged() {
                super.onChanged();

                mBtnDone.setText(mAttAdapter.getCount() > 1 ?
                        getString(R.string.caption_send) : getString(R.string.caption_save));
            }
        });

        mChkAllDay.setChecked(mMeeting.IsAllDay);
        setTimeEnabled();
        mBtnDone.setEnabled(mEditTitle.getText().length() > 0);
    }

    private void populateRecurrence() {
        boolean isRecurrent = mRecurrence != null;

        if (isRecurrent) {
            mRecurrentDate.setText(DateFmt.buildRecurrentDate(this, mRecurrence));
        }
        mTextDate.setVisibility(isRecurrent ? View.GONE : View.VISIBLE);
        mRecurrentDate.setVisibility(isRecurrent ? View.VISIBLE : View.GONE);
    }

    public void onSuggestions(View v) {
        Meeting meeting = createMeetingFromView();    // collect current data
        mTimeSuggestionActivity.start(meeting);
    }

    public void onASAP(View v) {
        mAsapLoader.start();
    }

    private MeetingTimeCandidate selectEarliest(MeetingTimeCandidates slots) {
        MeetingTimeCandidate earliest = null;

        for (MeetingTimeCandidate c : slots.value) {
            if (isSlotEarlier(c, earliest)) {
                earliest = c;
            }
        }

        return earliest;
    }

    private boolean isSlotEarlier(MeetingTimeCandidate c1, MeetingTimeCandidate c2) {
        Date time1 = getDateTimeFromCandidate(c1);

        if (time1.before(Calendar.getInstance().getTime())) {
            return false;
        }

        if (c2 == null) {
            return true;
        }

        Date time2 = getDateTimeFromCandidate(c2);

        return time1.before(time2);
    }

    private Date getDateTimeFromCandidate(MeetingTimeCandidate c) {
        DateFormat timeFmt = DateFmt.instance("HH:mm:ss");

        try {
            Date start = timeFmt.parse(c.MeetingTimeSlot.Start.Time);
            Calendar timeSlot = Calendar.getInstance();
            timeSlot.setTime(start);

            Calendar dateTimeSlot = Calendar.getInstance();
            dateTimeSlot.set(Calendar.HOUR_OF_DAY, timeSlot.get(Calendar.HOUR_OF_DAY));
            dateTimeSlot.set(Calendar.MINUTE, timeSlot.get(Calendar.MINUTE));

            return dateTimeSlot.getTime();

        } catch (ParseException e) {
            ErrorLogger.log(e);
        }

        return Calendar.getInstance().getTime();
    }

    public void onDoneButtonClick(View v) {
        mCreateEvent.start();
    }
}
