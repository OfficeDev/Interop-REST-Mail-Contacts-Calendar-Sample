package com.microsoft.office365.meetingmgr;

import com.microsoft.office365.meetingmgr.Models.EventMessage;
import com.microsoft.office365.meetingmgr.Models.MailMessage;
import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.util.Collections;
import java.util.Comparator;
import java.util.Date;
import java.util.List;

/**
 * Base load handler for dealing with invitation messages.
 * Attempts to find the EventMessage for specified event and build
 * a response Message.
 */
public class InviteLoadHandlerBase extends LoadHandler<MailMessage> {
    // Max count of messages to consider
    private static final int MAX_MESSAGES = 20;

    protected String mAction;
    protected String mComment;  // used by derived classes
    protected Meeting mMeeting;

    @Override
    public HttpCallable onCreate() {
        mMeeting = getArg("event", Meeting.getClassDeducer());
        mAction = getArg("action").toLowerCase();
        mComment = getArg("comment").toLowerCase();

        return new HttpCallable() {
            @Override
            public MailMessage call(HttpHelper hp) throws Exception {
                Class<? extends EventMessage> cls = EventMessage.getClassToUse();
                List<? extends EventMessage> sentInvites = hp.getItems(buildFilterUri(), MAX_MESSAGES, cls);

                if (sentInvites == null) {
                    return null;
                }

                // Ensure most recent messages are on top
                Collections.sort(sentInvites, new Comparator<EventMessage>() {
                            @Override
                            public int compare(EventMessage lhs, EventMessage rhs) {
                                Date lhsDate = DateFmt.dateFromApiDateString(lhs.getTimeCreated());
                                Date rhsDate = DateFmt.dateFromApiDateString(rhs.getTimeCreated());

                                return rhsDate.compareTo(lhsDate);
                            }
                        });

                EventMessage invite = findSpecificInvite(sentInvites);

                if (invite == null) {
                    Manager.Instance.showToast("No messages to reply to...");
                    return null;
                }

                return hp.postItem(buildResponseUri(invite.Id), cls);
            }
        };
    }

    @Override
    public void onFinished(MailMessage replyMessage) {
    }

    private String buildFilterUri() {
        StringBuilder sb = new StringBuilder(OData.getFoldersPath());
        sb.append(mMeeting.IsOrganizer ? "/SentItems/" : "/Inbox/");
        sb.append(OData.getMessagesPath());
        sb.append("?$filter=");

        buildFilter(sb, mMeeting.Subject);

        return sb.toString();
    }

    private void buildFilter(StringBuilder sb, String subject) {
        sb.append(String.format("Subject eq '%s'", subject));
        addTimeStamp(sb);
    }

    private void addTimeStamp(StringBuilder sb) {
        // We assume that the property name for EventMessage is the same as for Event
        String propertyName = mMeeting.getCreatedTimePropertyName();

        sb.append(" and ");
        sb.append(propertyName).append(" gt ");
        sb.append(mMeeting.getCreatedTime());
    }

    private String buildResponseUri(String messageId) {
        String uri = "messages/" + messageId + '/';

        switch (mAction) {
            case OData.REPLY:
                uri += OData.CREATE_REPLY;
                break;
            case OData.REPLY_ALL:
                uri += OData.CREATE_REPLY_ALL;
                break;
            case OData.FORWARD:
                uri += OData.CREATE_FORWARD;
                break;
        }
        return uri;
    }

    private EventMessage findSpecificInvite(List<? extends EventMessage> sentInvites) {
        for (EventMessage em : sentInvites) {
             if (isEventMessage(em)) {
                if (HttpHelper.isUnified() || doesMessageMatch(em)) {
                    return em;
                }
             }
        }
        return null;
    }

    private boolean doesMessageMatch(EventMessage em) {
        return !Utils.isNullOrEmpty(em.NavigationLink) &&
                em.NavigationLink.contains(mMeeting.Id);
    }

    private boolean isEventMessage(EventMessage em) {
        return !Utils.isNullOrEmpty(em.Type) &&
                em.Type.equalsIgnoreCase(em.getEventMessageType());
    }
}
