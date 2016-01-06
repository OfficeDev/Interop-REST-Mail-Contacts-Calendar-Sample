package com.microsoft.office365.meetingmgr.Models;

import java.util.List;

/**
 * Representation of meeting time candidate list
 */
public class MeetingTimeCandidates {
    public List<MeetingTimeCandidate> value;
    private MeetingTimeCandidates() {}

    public MeetingTimeCandidates(List<MeetingTimeCandidate> list) {
        value = list;
    }
}

