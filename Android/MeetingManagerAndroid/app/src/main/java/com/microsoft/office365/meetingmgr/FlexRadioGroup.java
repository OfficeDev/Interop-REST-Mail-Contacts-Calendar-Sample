package com.microsoft.office365.meetingmgr;

import android.app.Activity;
import android.view.View;
import android.view.ViewParent;
import android.widget.RadioButton;
import android.widget.RadioGroup;

import java.util.ArrayList;
import java.util.List;

/**
 * Radio group supporting non-adjucent radio buttons
 */
public class FlexRadioGroup {
    private List<RadioButton> mButtons = new ArrayList<>();
    private final View mGroup;

    public FlexRadioGroup(Activity activity, int... radiosIDs) {
        // Get the root view of the activity
        mGroup = activity.findViewById(android.R.id.content);

        for (int radioButtonID : radiosIDs) {
            RadioButton rb = (RadioButton) mGroup.findViewById(radioButtonID);
            if (rb != null) {
                mButtons.add(rb);
                rb.setOnClickListener(mOnClick);
            }
        }
        mButtons.get(0).setChecked(true);
    }

    public void setChecked(int radioButtonId) {
        doSetChecked(mGroup.findViewById(radioButtonId));
    }

    private void doSetChecked(View v) {
        for (RadioButton rb : mButtons) {
            ViewParent p = rb.getParent();

            if (p instanceof RadioGroup) {
                // if RadioButton belongs to RadioGroup,
                // then deselect all radios in it
                RadioGroup rg = (RadioGroup) p;
                rg.clearCheck();
            } else if (rb != v) {
                // if RadioButton DOES NOT belong to RadioGroup,
                // just deselect it
                rb.setChecked(false);
            }
        }

        // now let's select currently clicked RadioButton
        RadioButton rb = (RadioButton) v;
        rb.setChecked(true);
    }

    View.OnClickListener mOnClick = new View.OnClickListener() {

        @Override
        public void onClick(View v) {
            doSetChecked(v);
        }
    };

    /**
     ** Returns the Id of the radio button that is checked or -1 if none are checked
     **/
    public int getCheckedRadioButtonId() {
        for (RadioButton rb : mButtons) {
            if (rb.isChecked()) {
                return rb.getId();
            }
        }
        return -1;
    }
}
