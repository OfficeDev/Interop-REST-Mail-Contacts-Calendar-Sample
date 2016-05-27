/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
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
