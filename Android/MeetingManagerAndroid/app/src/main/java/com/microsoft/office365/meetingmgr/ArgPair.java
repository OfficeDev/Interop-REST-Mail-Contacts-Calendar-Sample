package com.microsoft.office365.meetingmgr;

import android.util.Pair;

/**
 * Representation of arguments
 */
public class ArgPair extends Pair<String, Object> {
    ArgPair(String key, Object value) {
        super(key, value);
    }
}
