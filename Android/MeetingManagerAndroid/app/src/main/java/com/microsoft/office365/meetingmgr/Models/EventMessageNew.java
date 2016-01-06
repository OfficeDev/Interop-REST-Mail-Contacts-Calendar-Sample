package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of event message in Graph API
 */
public class EventMessageNew extends EventMessage {
    public String createdDateTime;

    @Override
    public String getTimeCreated() {
        return createdDateTime;
    }

    @Override
    public String getEventMessageType() {
        return "#microsoft.graph.eventMessage";
    }
}
