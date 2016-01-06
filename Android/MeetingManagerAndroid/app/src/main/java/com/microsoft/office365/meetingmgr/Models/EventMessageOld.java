package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of event message in Office365 API
 */
public class EventMessageOld extends EventMessage {
    public String DateTimeCreated;

    @Override
    public String getTimeCreated() {
        return DateTimeCreated;
    }

    @Override
    public String getEventMessageType() {
        return "#Microsoft.OutlookServices.EventMessage";
    }
}
