/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.CheckBox;
import android.widget.EditText;

/**
 * Handles modification of app settings
 */
public class RegistrationActivity extends BaseActivity {
    private EditText mEditClientId;
    private EditText mEditRedirectUri;
    private CheckBox mChkIsUnified;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        mEditClientId = (EditText) findViewById(R.id.clientId);
        mEditRedirectUri = (EditText) findViewById(R.id.redirectUri);

        mEditClientId.setText(Manager.Instance.getClientId());
        mEditRedirectUri.setText(Manager.Instance.getRedirectUri());

        mChkIsUnified = (CheckBox) findViewById(R.id.isUnified);

        mChkIsUnified.setChecked(HttpHelper.isUnified());
    }

    public void onSave(View view) {
        HttpHelper.setUnified(mChkIsUnified.isChecked());

        Manager.Instance.saveRegistration(
                mEditClientId.getText().toString(),
                mEditRedirectUri.getText().toString());

        final Intent intent = new Intent(getApplicationContext(), ConnectActivity.class);
        startActivity(intent);
    }
}
