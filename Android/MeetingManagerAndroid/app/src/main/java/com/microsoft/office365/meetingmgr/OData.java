package com.microsoft.office365.meetingmgr;

/**
 * Constants for Office365 OData implementation
 */
public final class OData {
    private OData() {
    }

    public final static String REPLY = "reply";
    public final static String REPLY_ALL = "replyall";
    public final static String FORWARD = "forward";
    public final static String ACCEPT = "accept";
    public final static String DECLINE = "decline";
    public final static String TENTATIVELY_ACCEPT = "tentativelyaccept";
    public final static String CREATE_REPLY = "createreply";
    public final static String CREATE_REPLY_ALL = "createreplyall";
    public final static String CREATE_FORWARD = "createforward";
//    public final static String SEND_MAIL = "sendmail";
    public final static String SEND = "send";

    public final static String SUNDAY = "sunday";
    public final static String MONDAY = "monday";
    public final static String TUESDAY = "tuesday";
    public final static String WEDNESDAY = "wednesday";
    public final static String THURSDAY = "thursday";
    public final static String FRIDAY = "friday";
    public final static String SATURDAY = "saturday";

    public final static String NO_END = "noend";
    public final static String END_BY = "enddate";
    public final static String END_AFTER = "numbered";

    public final static String DAILY = "daily";
    public final static String WEEKLY = "weekly";
    public final static String ABSOLUTE_MONTHLY = "absolutemonthly";
    public final static String RELATIVE_MONTHLY = "relativemonthly";
    public final static String ABSOLUTE_YEARLY = "absoluteyearly";
    public final static String RELATIVE_YEARLY = "relativeyearly";

    public final static String FIRST = "first";
    public final static String SECOND = "second";
    public final static String THIRD = "third";
    public final static String FOURTH = "fourth";
    public final static String LAST = "last";

//    public final static String SERIES_MASTER = "seriesmaster";
//    public final static String SINGLE_INSTANCE = "singleinstance";

    public final static String OCCURRENCE = "occurrence";
    public final static String EXCEPTION = "exception";

    public final static String ACCEPTED = "accepted";
    public final static String TENTATIVELY_ACCEPTED = "tentativelyaccepted";
//    public final static String NOT_RESPONDED = "notresponded";
    public final static String REQUIRED = "required";
    public final static String NONE = "none";

    public static String getFoldersPath() {
        return HttpHelper.isUnified() ? "MailFolders" : "Folders";
    }

    public static String getMessagesPath() {
        return "Messages";
    }
}
