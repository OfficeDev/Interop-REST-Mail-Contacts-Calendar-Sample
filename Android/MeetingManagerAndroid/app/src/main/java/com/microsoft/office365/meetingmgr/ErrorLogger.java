/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.util.Log;

/**
 * Logs app errors/exceptions
 */
public class ErrorLogger {
    public static void log(Exception e) {
        e.printStackTrace();

        if (Manager.Instance != null) {
            Manager.Instance.showToast(e.getMessage());
        }
    }

    public static void log(String message) {
        Log.i("ERRORMSG", message);
        if (Manager.Instance != null) {
            Manager.Instance.showToast(message);
        }
    }
}
