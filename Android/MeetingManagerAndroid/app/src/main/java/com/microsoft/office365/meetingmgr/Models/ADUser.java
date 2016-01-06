package com.microsoft.office365.meetingmgr.Models;

import com.microsoft.office365.meetingmgr.Utils;

/**
 * Representation of AAD user
 */
public class ADUser {
    public String userPrincipalName;
    public String displayName;
    public String givenName;
    public String surName;

    @Override
    public String toString() {
        if (!Utils.isNullOrEmpty(displayName)) {
            return displayName;
        }
        return userPrincipalName;
    }
}
