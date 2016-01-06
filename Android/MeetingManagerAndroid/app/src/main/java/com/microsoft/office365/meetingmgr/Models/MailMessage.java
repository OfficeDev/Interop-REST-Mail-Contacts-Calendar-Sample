package com.microsoft.office365.meetingmgr.Models;

import java.util.ArrayList;
import java.util.List;

/**
 * Representation of email message
 */
public class MailMessage {
    public String Id;
    public String Subject;
    public Body Body = new Body();
    public List<Recipient> ToRecipients = new ArrayList<Recipient>();
    public Sender Sender = new Sender();

    public MailMessage() {
    }

    public MailMessage(String content, String subject) {
        Body.Content = content;
        this.Subject = subject;
    }

    public void addRecipient(String address) {
        ToRecipients.add(new Recipient(address));
    }

    public static class Body {
        public String Content;
        public String ContentType = "HTML";
    }

    public static class Recipient {
        public EmailAddress EmailAddress = new EmailAddress();

        private Recipient() {}

        public Recipient(String address) {
            EmailAddress.Address = address;
        }

        public Recipient(EmailAddress emailAddress) {
            this.EmailAddress  = emailAddress;
        }
        @Override
        public String toString() {
            return EmailAddress.toString();
        }
    }

    private static class Sender {
        public EmailAddress EmailAddress = new EmailAddress();
    }
}
