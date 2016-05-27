/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.os.Bundle;
import android.text.Html;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;

import com.microsoft.office365.meetingmgr.Models.Attendee;
import com.microsoft.office365.meetingmgr.Models.MailMessage;
import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.util.ArrayList;

/**
 * Handles sending response emails
 *
 * Uses the following REST APIs:
 * 1. Update message
 * 2. Send message
 * 3. Delete message
 */
public class SendMailActivity extends BaseActivity {
    private Button mBtnSend;
    private Button mBtnTo;
    private TextView mTextSubject;
    private EditText mEditComment;
    private TextView mTextMessageCaption;
    private TextView mTextMessage;
    private View mViewMessage;
    private ListView mListRecipients;

    private String mAction;
    private RecipientsAdapter mAdapter;
    private MailMessage mReplyMessage;

    private ActivityHolder mAttendeeActivity;
    private ActivityHolder mContactsActivity;
    private boolean mMailHasBeenSent;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_send_mail);

        Meeting meeting = getArg("event", Meeting.getClassDeducer());
        mAction = getArg("action").toLowerCase();

        setTitle(getTitleFromType());
        initViews();

        if (isEmailAction()) {
            mReplyMessage = getArg("message", MailMessage.class);

            populateViews(mReplyMessage.Subject, mReplyMessage.Body.Content);
            mAdapter = new RecipientsAdapter(mListRecipients, mReplyMessage.ToRecipients);
        } else {
            populateViews(meeting.Subject, meeting.Body.Content);

            mAdapter = new RecipientsAdapter(mListRecipients, new ArrayList<MailMessage.Recipient>());
            // We only need it for display
            mAdapter.add(new MailMessage.Recipient(meeting.Organizer.EmailAddress));

            mListRecipients.setEnabled(false);
            mBtnTo.setEnabled(false);
            mViewMessage.setVisibility(View.GONE);
            mTextMessageCaption.setVisibility(View.GONE);
         }

        onRecipientsChanged();

        bindMenus();
        setEventListeners();
        registerActivities();
    }

    private void bindMenus() {

        bindPopupMenu(mBtnTo, R.menu.attendee_menu, new PopupMenuHandler() {
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

        if (isEmailAction()) {
            bindContextMenu(mListRecipients, R.menu.recipient_context_menu, new ContextMenuHandler<MailMessage.Recipient>() {
                @Override
                public boolean onItemSelected(int itemId, MailMessage.Recipient object) {
                    switch (itemId) {
                        case R.id.remove:
                            mAdapter.remove(object);
                            onRecipientsChanged();
                            return true;
                    }
                    return false;
                }
            });
        }
    }

    private String getTitleFromType() {
        switch (mAction.toLowerCase()) {
            case OData.REPLY_ALL:
                return getString(R.string.title_sendMail_replyAll);
            case OData.REPLY:
                return getString(R.string.title_sendMail_reply);
            case OData.FORWARD:
                return getString(R.string.title_sendMail_forward);
            case OData.ACCEPT:
                return getString(R.string.title_sendMail_accept);
            case OData.TENTATIVELY_ACCEPT:
                return getString(R.string.title_sendMail_tentativelyAccept);
            case OData.DECLINE:
                return getString(R.string.title_sendMail_decline);
        }
        return "???";
    }

    private void populateViews(String subject, String content) {
        mEditComment.setText(getArg("comment"));
        mTextSubject.setText(subject);
        String cleanContent = Utils.stripHtmlComments(content);

        if (Utils.isHtml(cleanContent)) {
            mTextMessage.setText(Html.fromHtml(cleanContent));
        } else {
            mTextMessage.setText(cleanContent);
        }
    }

    private void initViews() {
        mBtnSend = (Button) findViewById(R.id.send);
        mBtnTo = (Button) findViewById(R.id.to);
        mTextSubject = (TextView) findViewById(R.id.mailSubject);
        mEditComment = (EditText) findViewById(R.id.comment);
        mTextMessage = (TextView) findViewById(R.id.message);
        mViewMessage = findViewById(R.id.messageView);
        mTextMessageCaption = (TextView) findViewById(R.id.message_caption);
        mListRecipients = (ListView) findViewById(R.id.recipients);
    }

    private void setEventListeners() {
        mBtnSend.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                send();
            }
        });
    }

    private void registerActivities() {
        mAttendeeActivity = registerActivity(AddAttendeeActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                Attendee att = getArg(Attendee.class);
                mAdapter.add(new MailMessage.Recipient(att.EmailAddress));
                onRecipientsChanged();
            }
        });

        mContactsActivity = registerActivity(ContactsActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                Attendee att = getArg(Attendee.class);
                mAdapter.add(new MailMessage.Recipient(att.EmailAddress));
                onRecipientsChanged();
            }
        });
    }

    @Override
    public void onDestroy() {
        if (!mMailHasBeenSent && isEmailAction()) {
            deleteDraft();
        }
        super.onDestroy();
    }

    private void deleteDraft() {
        Utils.forkRunnable(new Runnable() {
            @Override
            public void run() {
                String uri = OData.getFoldersPath() + "/Drafts/messages/" + mReplyMessage.Id;
                new HttpHelper().deleteItem(uri);
            }
        });
   }

    private void onRecipientsChanged() {
        mBtnSend.setEnabled(!mAdapter.isEmpty());
    }

    private void send() {
        if (isEmailAction()) {
            sendMailAndFinish();
        } else {
            finishWithComment();
        }
    }

    private boolean isEmailAction() {
        switch (mAction) {
            case OData.REPLY:
            case OData.REPLY_ALL:
            case OData.FORWARD:
                return true;
        }
        return false;
    }

    private void finishWithComment() {
        finish(
                new ArgPair("comment", mEditComment.getText().toString()),
                new ArgPair("action", mAction)
        );
    }

    private void sendMailAndFinish() {
        ODataUtils.updateAndSendMessage(mReplyMessage, mEditComment.getText().toString(), mAdapter.getList());

        mMailHasBeenSent = true;
        finish();
    }
}
