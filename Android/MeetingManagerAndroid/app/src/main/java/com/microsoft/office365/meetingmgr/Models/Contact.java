package com.microsoft.office365.meetingmgr.Models;

import android.graphics.Bitmap;
import com.fasterxml.jackson.annotation.JsonIgnore;

/**
 * Representation of Contact
 */
public class Contact {
    public String Id;
    public EmailAddress[] EmailAddresses;
    public String DisplayName;

    @JsonIgnore
    public byte[] photoData;

    @JsonIgnore
    public Bitmap photo;

    @Override
    public String toString() {
        return DisplayName;
    }
}
