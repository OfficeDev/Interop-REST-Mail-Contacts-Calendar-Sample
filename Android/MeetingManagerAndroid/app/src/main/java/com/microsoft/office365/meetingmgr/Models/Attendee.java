package com.microsoft.office365.meetingmgr.Models;

/**
 * Representation of Meeting attendee
 */
public class Attendee {
    public EmailAddress EmailAddress = new EmailAddress();
    public ResponseStatus Status;
    public String Type;

    Attendee() {}

    public Attendee(ADUser user) {
        EmailAddress.Address = user.userPrincipalName;
        EmailAddress.Name = user.displayName;
    }

    public Attendee(EmailAddress address) {
        EmailAddress= address;
    }

    @Override
    public String toString() {
        return EmailAddress.toString();
    }
}
