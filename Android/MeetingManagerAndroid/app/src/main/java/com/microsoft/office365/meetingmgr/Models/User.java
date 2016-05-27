/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of logged-in Office365 user
 */
public class User {
    public String id;
    public String DisplayName;
    public String Alias;
    public String MailboxGuid;

    private User() {}

    public User(ADUser adUser) {
        id = adUser.userPrincipalName;
        DisplayName = adUser.displayName;
    }
}
