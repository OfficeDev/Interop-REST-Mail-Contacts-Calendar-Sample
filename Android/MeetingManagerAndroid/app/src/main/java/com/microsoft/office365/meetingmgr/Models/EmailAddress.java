package com.microsoft.office365.meetingmgr.Models;

import com.microsoft.office365.meetingmgr.Utils;

/**
 * Representation of email address
 */
public class EmailAddress {
    public String Address;
    public String Name;

    @Override
    public String toString() {
        if (!Utils.isNullOrEmpty(Name)) {
            return Name;
        }
        return Address;
    }
}
