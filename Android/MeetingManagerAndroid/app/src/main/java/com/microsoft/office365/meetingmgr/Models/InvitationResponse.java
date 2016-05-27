/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of response to event invitation
 */
public class InvitationResponse {
    public final String Comment;
    public final boolean SendResponse;

    public InvitationResponse(String comment) {
        Comment = comment;
        SendResponse = true;
    }

    public InvitationResponse() {
        Comment = null;
        SendResponse = false;
    }
}
