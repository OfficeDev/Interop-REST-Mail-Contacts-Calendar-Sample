package com.microsoft.office365.meetingmgr.Models;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Representation of file attachment
 */
public class FileAttachment {
    @JsonProperty("@odata.type")
    public String Type = "#Microsoft.OutlookServices.FileAttachment";
    public final String ContentBytes;
    public final String Name;

    private FileAttachment() {
        this(null, null);
    }

    public FileAttachment(String content, String name) {
        ContentBytes = content;
        Name = name;
    }
}
