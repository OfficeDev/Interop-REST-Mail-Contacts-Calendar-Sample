package com.microsoft.office365.meetingmgr;

/**
 * Return object class based on its json representation
 */
public interface GetClass {
    Class get(String json);
}
