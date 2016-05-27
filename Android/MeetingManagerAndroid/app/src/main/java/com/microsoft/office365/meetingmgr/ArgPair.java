/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
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
