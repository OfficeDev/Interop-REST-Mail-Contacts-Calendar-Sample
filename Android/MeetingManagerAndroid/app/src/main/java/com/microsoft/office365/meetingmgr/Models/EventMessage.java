/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.microsoft.office365.meetingmgr.HttpHelper;

/**
 * Base representation of event message
 */
public abstract class EventMessage extends MailMessage {
    @JsonProperty("@odata.type")
    public String Type;
    @JsonProperty("Event@odata.navigationLink")
    public String NavigationLink;
    public String MeetingMessageType;

    public abstract String getTimeCreated();
    public abstract String getEventMessageType();

    public static Class<? extends EventMessage> getClassToUse() {
        return HttpHelper.isUnified() ? EventMessageNew.class : EventMessageOld.class;
    }
}
