package com.microsoft.office365.meetingmgr;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;

/**
 * Base class for various text watches
 */
public class MultiTextWatcher implements TextWatcher {

    protected final View mView;
    public MultiTextWatcher(View view) {
        mView = view;
    }

    @Override
    public void beforeTextChanged(CharSequence charSequence, int i, int i1, int i2) {
    }

    @Override
    public void onTextChanged(CharSequence charSequence, int i, int i1, int i2) {
    }

    @Override
    public void afterTextChanged(Editable editable) {
    }
}
