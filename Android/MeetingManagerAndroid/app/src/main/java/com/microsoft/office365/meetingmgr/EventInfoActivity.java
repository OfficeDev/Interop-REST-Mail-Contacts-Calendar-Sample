/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.os.Bundle;
import android.text.Html;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ListView;
import android.widget.PopupMenu;
import android.widget.TextView;

import com.microsoft.office365.meetingmgr.Models.InvitationResponse;
import com.microsoft.office365.meetingmgr.Models.MailMessage;
import com.microsoft.office365.meetingmgr.Models.Meeting;
import com.microsoft.office365.meetingmgr.Models.MeetingNew;
import com.microsoft.office365.meetingmgr.Models.MeetingOld;

/**
 * Displays Meeting Details.
 * Handles responses to invitation and sending emails.
 * For meeting organizer, allows switching to Modify Meeting activity.
 *
 * Uses the following REST APIs:
 * 1. Respond to event
 * 2. Get messages
 * 3. Create draft message
 * 4. Update message
 * 5. Send message
 */
public class EventInfoActivity extends BaseActivity {
    private TextView mTextSubject;
    private TextView mTextDate;
    private TextView mTextBody;
    private TextView mTextLocation;
    private ListView mListAttendees;

    private Button mBtnEdit;

    private Button mBtnAccept;
    private Button mBtnDecline;
    private Button mBtnTentative;
    private Button mBtnEmail;
    private Button mBtnLate;

    private Meeting mMeeting;

    private LoaderHolder mFindInviteLoader;
    private LoaderHolder mFindInviteLoaderLate;

    private ActivityHolder mNewEventActivity;
    private ActivityHolder mSendMailActivity;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_event_info);

        mMeeting = getArg(Meeting.getClassDeducer());

        initViews();
        populateControls();

        registerLoaders();
        registerActivities();
        registerEventListeners();
        bindMenus();
    }

    private void initViews() {
        mTextSubject = (TextView) findViewById(R.id.subject);
        mTextDate = (TextView) findViewById(R.id.date);
        mTextBody = (TextView) findViewById(R.id.body);
        mTextLocation = (TextView) findViewById(R.id.location);
        mListAttendees = (ListView) findViewById(R.id.attendees);
        mBtnEdit = (Button) findViewById(R.id.editEvent);
        mBtnAccept = (Button) findViewById(R.id.accept);
        mBtnDecline = (Button) findViewById(R.id.decline);
        mBtnTentative = (Button) findViewById(R.id.tentative);
        mBtnEmail = (Button) findViewById(R.id.email);
        mBtnLate = (Button) findViewById(R.id.late);
    }

    private void registerActivities() {
        mNewEventActivity = registerActivity(NewMeetingActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                mMeeting = getArg(HttpHelper.isUnified() ? MeetingNew.class : MeetingOld.class);
                populateControls();
            }
        });

        mSendMailActivity = registerActivity(SendMailActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                executeAndFinish(getArg("action"), new InvitationResponse(getArg("comment")));
            }
        });
    }

    private void registerLoaders() {
        mFindInviteLoader = registerLoadHandler(new InviteLoadHandler());
        mFindInviteLoaderLate = registerLoadHandler(new InviteLoadHandlerLate());
    }

    private void populateControls() {
        mTextSubject.setText(mMeeting.Subject);

        String date;

        if (mMeeting.Recurrence != null) {
            date = DateFmt.buildRecurrentDate(this, mMeeting.Recurrence);
        } else {
            date = DateFmt.toDateString(mMeeting.getStart());
        }

        String time = String.format(getString(R.string.time_interval),
                DateFmt.toTimeString(mMeeting.getStart()),
                DateFmt.toTimeString(mMeeting.getEnd())
        );

        String dateTime = String.format("%s %s", date, time);

        mTextDate.setText(dateTime);

        String description = Utils.stripHtmlComments(mMeeting.Body.Content);
        mTextBody.setText(Html.fromHtml(description));

        mTextLocation.setText(Utils.isNullOrEmpty(mMeeting.Location.DisplayName) ?
                getString(R.string.location_not_specified) : mMeeting.Location.DisplayName);

        mMeeting.bindAttendeeList(mListAttendees);

        if (!mMeeting.IsOrganizer || mMeeting.IsCancelled) {
            mBtnEdit.setVisibility(View.GONE);
        } else {
            mBtnEdit.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    mNewEventActivity.start(mMeeting);
                }
            });
        }

        mBtnLate.setVisibility(mMeeting.hasAccepted() ? View.VISIBLE : View.GONE);

        if (mMeeting.IsOrganizer || mMeeting.IsCancelled) {
            mBtnAccept.setVisibility(View.GONE);
            mBtnDecline.setVisibility(View.GONE);
            mBtnTentative.setVisibility(View.GONE);

        } else if (mMeeting.hasAccepted()) {
            mBtnAccept.setVisibility(View.GONE);
            mBtnTentative.setVisibility(View.GONE);
        }
    }

    private void registerEventListeners() {
        mBtnLate.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                mFindInviteLoaderLate.start(
                        new ArgPair("event", mMeeting),
                        new ArgPair("action", OData.REPLY_ALL),
                        new ArgPair("comment", getString(R.string.running_late))
                );
            }
        });
    }

    private void bindMenus() {
        bindPopupMenu(mBtnEmail, R.menu.email_menu, new PopupMenuHandler() {
            @Override
            public void onCreate() {
                if (mMeeting.IsOrganizer) {
                    findItem(R.id.reply).setVisible(false);
                }
            }

            @Override
            public boolean onItemSelected(int itemId) {
                switch (itemId) {
                    case R.id.reply:
                        sendMail(OData.REPLY);
                        return true;
                    case R.id.replyAll:
                        sendMail(OData.REPLY_ALL);
                        return true;
                    case R.id.forward:
                        sendMail(OData.FORWARD);
                        return true;
                }
                return false;
            }
        });
    }

    public void onAccept(View v) {
        askForEdit(v, OData.ACCEPT);
    }

    public void onTentative(View v) {
        askForEdit(v, OData.TENTATIVELY_ACCEPT);
    }

    public void onDecline(View v) {
        askForEdit(v, OData.DECLINE);
    }

    private void askForEdit(View v, final String action) {
        PopupMenu popup = new PopupMenu(this, v);
        popup.inflate(R.menu.invitation_response_menu);

        popup.setOnMenuItemClickListener(new PopupMenu.OnMenuItemClickListener() {
            @Override
            public boolean onMenuItemClick(MenuItem item) {
                switch (item.getItemId()) {
                    case R.id.edit:
                        sendAcceptOrDecline(action);
                        return true;
                    case R.id.send:
                        executeAndFinish(action, null);
                        return true;
                    case R.id.go:
                        executeAndFinish(action, new InvitationResponse());
                        return true;
                    default:
                        return false;
                }
            }
        });

        popup.show();
    }

    private void sendAcceptOrDecline(String action) {
        mSendMailActivity.start(
                new ArgPair("event", mMeeting),
                new ArgPair("action", action)
        );
    }

    private void sendMail(String action/*, String comment*/) {
        mFindInviteLoader.start(
                new ArgPair("event", mMeeting),
                new ArgPair("action", action),
                new ArgPair("comment", "")
        );
    }

    private void executeAndFinish(final String action, final InvitationResponse response) {
        Utils.forkRunnable(new Runnable() {
            @Override
            public void run() {
                new HttpHelper().postItemVoid(buildUri(action), response);
            }
        });

        finish();
    }

    private String buildUri(String action) {
        StringBuilder sb = new StringBuilder("events/");
        sb.append(mMeeting.Id).append('/').append(action);
        return sb.toString();
    }

    private class InviteLoadHandler extends InviteLoadHandlerBase {
        @Override
        public void onFinished(MailMessage replyMessage) {
            mSendMailActivity.start(
                    new ArgPair("event", mMeeting),
                    new ArgPair("message", replyMessage),
                    new ArgPair("action", mAction),
                    new ArgPair("comment", mComment)
            );
        }
    }

    private class InviteLoadHandlerLate extends InviteLoadHandlerBase {
        @Override
        public void onFinished(MailMessage replyMessage) {
            ODataUtils.updateAndSendMessage(replyMessage, mComment, null);
        }
    }
}
