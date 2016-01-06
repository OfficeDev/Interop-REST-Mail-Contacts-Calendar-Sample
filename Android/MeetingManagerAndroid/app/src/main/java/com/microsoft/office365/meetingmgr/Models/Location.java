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
