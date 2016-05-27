/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of meeting location
 */
public class Location {
    public String DisplayName;
    public String LocationEmailAddress;

    public Location(String displayName) {
        DisplayName = displayName;
    }

    public Location() {
    }
}
