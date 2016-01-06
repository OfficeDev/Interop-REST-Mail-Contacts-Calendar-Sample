package com.microsoft.office365.meetingmgr;

import android.content.Context;

public class WaitDialog {
    private static SpinnerDialog mSpinner;
//    private static ProgressDialog mSpinner2;

    public static void show(Context ctx) {
        mSpinner = new SpinnerDialog(ctx);
        mSpinner.show();
//        mSpinner2 = new ProgressDialog(ctx);
//        mSpinner2.show();
    }

    public static void hide() {
        mSpinner.dismiss();
//        mSpinner2.dismiss();
    }
}
