/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of response to the meeting invitation
 */
public class ResponseStatus {
    public String Response;
    public String Time;

    private ResponseStatus() {}
    public ResponseStatus(String response) {
        Response = response;
    }
}
