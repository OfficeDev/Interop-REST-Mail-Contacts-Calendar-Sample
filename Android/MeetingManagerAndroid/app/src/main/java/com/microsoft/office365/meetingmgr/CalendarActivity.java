package com.microsoft.office365.meetingmgr;

import android.content.DialogInterface;
import android.os.Bundle;
import android.view.MenuItem;
import android.view.View;
import android.widget.CalendarView;
import android.widget.ListView;
import android.widget.TextView;

import com.microsoft.office365.meetingmgr.Models.MailMessage;
import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.TimeZone;

/**
 * Handles calendar navigation and shows meeting list for selected date.
 * In the meeting list, allows selection of either a series or a single event.
 * Handles meeting cancellation and a switch to Meeting Details or Meeting Create/Edit activities.
 *
 * Uses the following REST APIs:
 * 1. Get calendar view
 * 2. Get event
 * 3. Delete event
 * 4. Get messages
 * 5. Create draft message
 * 6. Update message
 * 7. Send message
 */
public class CalendarActivity extends BaseActivity {
    private final static int MAX_DAILY_EVENTS = 20;

    private CalendarView mCalendar;
    private TextView mTextCurrentDate;
    private ListView mListMeetings;

    private Calendar mCurrentDate;

    private LoaderHolder mUpdateLoader;
    private LoaderHolder mDeleteLoader;
    private LoaderHolder mGetEventLoader;
    private LoaderHolder mGetEventEditLoader;
    private LoaderHolder mFindInviteLoader;

    private ActivityHolder mEventInfoActivity;
    private ActivityHolder mNewEventActivity;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar);

        initViews();
        setEventHandlers();
        bindContextMenus();

        registerLoaders();
        initializeCalendar();

        registerActivities();

        mListMeetings.addHeaderView(buildHeaderView(mListMeetings));
    }

    private void initViews() {
        mCalendar = (CalendarView) findViewById(R.id.calendar);
        mTextCurrentDate = (TextView) findViewById(R.id.selectedDate);
        mListMeetings = (ListView) findViewById(R.id.meetings);
    }

    private void setEventHandlers() {
        mListMeetings.setOnItemClickListener(new OnListItemClickListener<Meeting>() {
            @Override
            public void onClick(Meeting meeting) {
                switchToEventInfo(meeting);
            }
        });
    }

    private void registerActivities() {
        mEventInfoActivity = registerActivity(EventInfoActivity.class);

        mNewEventActivity = registerActivity(NewMeetingActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                startUpdateLoader();
            }
        });
    }

    private void startUpdateLoader() {
          mUpdateLoader.start();
    }

    private void bindContextMenus() {
        bindContextMenu(mListMeetings, new ContextMenuHandler<Meeting>() {
            @Override
            public void onCreate(Meeting meeting) {
                if (meeting.isPartOfSeries()) {
                    inflate(R.menu.top_event_context_menu);
                } else {
                    inflate(R.menu.event_context_menu);
                }

                setItemVisibility(R.id.cancel, meeting.IsOrganizer);
                setItemVisibility(R.id.instance_cancel, meeting.IsOrganizer);
                setItemVisibility(R.id.master_cancel, meeting.IsOrganizer);
                setItemVisibility(R.id.edit, meeting.IsOrganizer);
                setItemVisibility(R.id.instance_edit, meeting.IsOrganizer);
                setItemVisibility(R.id.master_edit, meeting.IsOrganizer);
                setItemVisibility(R.id.late, meeting.hasAccepted());
                setItemVisibility(R.id.instance_late, meeting.hasAccepted());
            }

            private void setItemVisibility(int id, boolean isOrganizer) {
                MenuItem item = findItem(id);
                if (item != null) {
                    item.setVisible(isOrganizer);
                }
            }

            @Override
            public boolean onItemSelected(int itemId, Meeting meeting) {
                switch (itemId) {
                    case R.id.master:
                    case R.id.instance:
                        return true;

                    case R.id.master_view:
                        switchToEventInfo(meeting);
                        return true;

                    case R.id.view:
                    case R.id.instance_view:
                        onViewEvent(meeting);
                        return true;

                    case R.id.edit:
                    case R.id.instance_edit:
                    case R.id.master_edit:
                        onEditEvent(meeting, itemId == R.id.master_edit);
                        return true;

                    case R.id.late:
                    case R.id.instance_late:
                        onRunningLate(meeting);
                        return true;

                    case R.id.cancel:
                    case R.id.instance_cancel:
                    case R.id.master_cancel:
                        cancelMeeting(meeting, itemId == R.id.master_cancel);
                        return true;
                }
                return false;
            }
        });
    }

    private String buildCalendarUri() {
        Calendar startDate = (Calendar) mCurrentDate.clone();
        startDate.set(Calendar.HOUR_OF_DAY, 0);
        startDate.add(Calendar.MINUTE, 1);

        Calendar endDate = (Calendar) startDate.clone();
        endDate.add(Calendar.HOUR_OF_DAY, 24);

        String start = DateFmt.toApiUtcString(startDate.getTime());
        String end = DateFmt.toApiUtcString(endDate.getTime());

        String uri = String.format("calendarview?startDateTime=%s&endDateTime=%s", start, end);
        uri += "&$orderby=start";

        if (HttpHelper.isUnified()) {
            uri += "/dateTime";
        }

        return uri;
    }

    private void registerLoaders() {
        mUpdateLoader = registerLoadHandler(new UpdateLoader<>(Meeting.getClassToUse()));

        mDeleteLoader = registerLoadHandler(new LoadHandler() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public Object call(HttpHelper hp) throws Exception {
                        String uri = "events/" + getArg();
                        hp.deleteItem(uri);
                        // Return nun-null to trigger onFinished
                        return "";
                    }
                };
            }

            @Override
            public void onFinished(Object data) {
                startUpdateLoader();
            }
        });

        mGetEventLoader = registerLoadHandler(new GetEventLoader<>(Meeting.getClassToUse()));
        mGetEventEditLoader = registerLoadHandler(new GetEventEditLoader<>(Meeting.getClassToUse()));

        mFindInviteLoader = registerLoadHandler(new InviteLoadHandler());
    }

    private class InviteLoadHandler extends InviteLoadHandlerBase {
        @Override
        public void onFinished(MailMessage replyMessage) {
            ODataUtils.updateAndSendMessage(replyMessage, mComment, null);
        }
    }

    private class GetEventLoader<T extends Meeting> extends LoadHandler<T> {
        private final Class<T> mClass;

        GetEventLoader(Class<T> cls) {
            mClass = cls;
        }

        @Override
        HttpCallable onCreate() {
            return new HttpCallable() {
                @Override
                public Meeting call(HttpHelper hp) throws Exception {
                    String uri = "events/" + getArg();
                    return hp.getItem(uri, mClass);
                }
            };
        }

        @Override
        void onFinished(T data) {
            onViewEvent(data);
        }
    }

    private class GetEventEditLoader<T extends Meeting> extends GetEventLoader<T> {
        GetEventEditLoader(Class<T> cls) {
            super(cls);
        }

        @Override
        void onFinished(T data) {
            startCreateModify(data);
        }
    }

    private class UpdateLoader<T extends Meeting> extends LoadHandler<List<T>> {
        private final Class<T> mClass;

        UpdateLoader(Class<T> cls) {
            mClass = cls;
        }

        @Override
        HttpCallable onCreate() {
            final String uri = buildCalendarUri();

            return new HttpCallable() {
                @Override
                public List<T> call(HttpHelper hp) throws Exception {
                    return hp.getItems(uri, MAX_DAILY_EVENTS, mClass);
                }
            };
        }

        @Override
        void onFinished(List<T> data) {
            List<Meeting> dataNew = new ArrayList<>();
            dataNew.addAll(data);

            new MeetingsAdapter(mListMeetings, dataNew);
        }
    }

    private View buildHeaderView(View list) {
        View header = View.inflate(list.getContext(), R.layout.meeting_header_row, null);

        ((TextView) header.findViewById(R.id.startTime)).setText(getString(R.string.header_start));
        ((TextView) header.findViewById(R.id.subject)).setText(getString(R.string.header_subject));
        ((TextView) header.findViewById(R.id.location)).setText(getString(R.string.header_location));
        ((TextView) header.findViewById(R.id.organizer)).setText(getString(R.string.header_organizer));

        return header;
    }

    private void cancelMeeting(final Meeting meeting, final boolean cancelSeries) {
        String message;
        if (cancelSeries) {
            message = String.format(getString(R.string.alert_msg_cancel_series), meeting.Subject);
        } else if (meeting.isPartOfSeries()) {
            message = String.format(getString(R.string.alert_msg_cancel_series_occurrence), meeting.Subject);
        } else {
            message = String.format(getString(R.string.alert_msg_cancel_instance), meeting.Subject);
        }

        getConfirmation(
                message,
                new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        if (which == DialogInterface.BUTTON_POSITIVE) {
                            doCancelMeeting(meeting, cancelSeries);
                        }
                    }
                });
    }

    private void doCancelMeeting(Meeting meeting, boolean cancelSeries) {
        if (cancelSeries && meeting.isPartOfSeries()) {
            if (meeting.SeriesMasterId != null) {
                mDeleteLoader.start(meeting.SeriesMasterId);
                return;
            }
        }
        mDeleteLoader.start(meeting.Id);
    }

    private void switchToEventInfo(Meeting meeting) {
        if (meeting.SeriesMasterId != null) {
            viewEventById(meeting.SeriesMasterId);
        } else {
            onViewEvent(meeting);
        }
    }

    private void onEditEvent(Meeting meeting, boolean editSeries) {
        if (editSeries && meeting.isPartOfSeries()) {
            if (meeting.SeriesMasterId != null) {
                editEventById(meeting.SeriesMasterId);
                return;
            }
        }
        startCreateModify(meeting);
    }

    private void onViewEvent(Meeting item) {
        mEventInfoActivity.start(item);
    }

    private void viewEventById(String eventId) {
        mGetEventLoader.start(eventId);
    }

    private void editEventById(String eventId) {
        mGetEventEditLoader.start(eventId);
    }

    private void initializeCalendar() {
        Calendar now = Calendar.getInstance();
        setCurrentDate(now.get(Calendar.YEAR), now.get(Calendar.MONTH), now.get(Calendar.DAY_OF_MONTH));

        mCalendar.setOnDateChangeListener(new CalendarView.OnDateChangeListener() {
            @Override
            public void onSelectedDayChange(CalendarView view, int year, int month, int day) {
                setCurrentDate(year, month, day);
            }
        });
    }

    private void setCurrentDate(int year, int month, int day) {
        mCurrentDate = new GregorianCalendar(year, month, day);

        String dateString = DateFmt.toDateWithDayOfWeekString(mCurrentDate.getTime());

        Calendar now = Calendar.getInstance();

        int diffDays = now.get(Calendar.DAY_OF_MONTH) - mCurrentDate.get(Calendar.DAY_OF_MONTH);
        String closestDay = "";

        if (diffDays == 0) {
            closestDay = getString(R.string.calendar_today);
        } else if (diffDays == 1) {
            closestDay = getString(R.string.calendar_yesterday);
        } else if (diffDays == -1) {
            closestDay = getString(R.string.calendar_tomorrow);
        }

        if (!Utils.isNullOrEmpty(closestDay)) {
            dateString = String.format("%s, %s", closestDay, dateString);
        }

        mTextCurrentDate.setText(dateString);

        startUpdateLoader();
    }

    public void onRefresh(View v) {
        startUpdateLoader();
    }

    public void onNewButtonClick(View v) {
        startCreateModify(null);
    }

    private void startCreateModify(Meeting meeting) {
        mNewEventActivity.start(meeting);
    }

    private void onRunningLate(Meeting meeting) {
        mFindInviteLoader.start(
                new ArgPair("event", meeting),
                new ArgPair("action", OData.REPLY_ALL),
                new ArgPair("comment", getString(R.string.running_late))
        );
    }
}
