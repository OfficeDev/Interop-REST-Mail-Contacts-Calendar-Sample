/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.content.Context;

public class WaitDialog {
    private static SpinnerDialog mSpinner;

    public static void show(Context ctx) {
        mSpinner = new SpinnerDialog(ctx);
        mSpinner.show();
    }

    public static void hide() {
        mSpinner.dismiss();
    }
}
