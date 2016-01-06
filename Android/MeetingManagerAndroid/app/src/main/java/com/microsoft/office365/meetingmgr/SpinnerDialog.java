package com.microsoft.office365.meetingmgr;

import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.view.Window;
import android.view.WindowManager;

/**
 * Displays "Wait.." indication; blocks input
 */
public class SpinnerDialog extends Dialog {
    public SpinnerDialog(Context ctx) {
        super(ctx, android.R.style.Theme_Translucent);
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL);

        setContentView(R.layout.loading);
    }
}
