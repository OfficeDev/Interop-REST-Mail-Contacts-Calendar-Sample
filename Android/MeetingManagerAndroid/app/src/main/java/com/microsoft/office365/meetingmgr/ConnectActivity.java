/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.app.AlertDialog;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

import com.microsoft.office365.meetingmgr.Models.ADUser;
import com.microsoft.office365.meetingmgr.Models.User;

import java.util.List;

/**
 * Starting activity of the app. Handles the connection to Office 365.
 * When it first starts it only displays a button to Connect to Office 365.
 * If there are no cached tokens, the user is required to sign in to Office 365.
 * If there are cached tokens, the app tries to reuse them.
 * The activity redirects the user to the CalendarActivity upon successful connection.
 */
public class ConnectActivity extends BaseActivity {
    private Button mBtnConnect;

    private LoaderHolder mGetUser;
    private LoaderHolder mGetUserUnified;
    private LoaderHolder mInitAAD;

    private ActivityHolder mLoginActivity;
    private ActivityHolder mCalendarActivity;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_connect);

        initViews();
        registerLoaders();
        registerActivities();
    }

    private void registerLoaders() {
        mGetUser = registerLoadHandler(new LoadHandler<User>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public User call(HttpHelper hp) throws Exception {
                        return hp.getItem("", User.class);
                    }
                };
            }

            @Override
            public void onFinished(User user) {
                startCalendar(user);
            }
        });

        mGetUserUnified = registerLoadHandler(new LoadHandler<ADUser>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public ADUser call(HttpHelper hp) throws Exception {
                        return hp.getItem("", ADUser.class);
                    }
                };
            }

            @Override
            public void onFinished(ADUser adUser) {
                startCalendar(new User(adUser));
            }
        });

        mInitAAD = registerLoadHandler(new LoadHandler<List<ADUser>>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public List<ADUser> call(HttpHelper hp) throws Exception {
                        String uri = "https://graph.windows.net/myorganization/users?$top=1&api-version=1.5";
                        return hp.getItems(uri, 1, ADUser.class);
                    }
                };
            }

            @Override
            public void onFinished(List<ADUser> data) {
            }
        });
    }

    private void startCalendar(User user) {
        Manager.Instance.setUser(user);
        Manager.Instance.showToast(String.format(getString(R.string.logged_in_as), user.id));

        mCalendarActivity.start();
        finish();   // don't return here
    }

    private void registerActivities() {
        mLoginActivity = registerActivity(LoginActivity.class, new ActivityHandler() {
            @Override
            public void onResult() {
                TokenHandler.setAuthCode(getArg());

                if (HttpHelper.isUnified()) {
                    mGetUserUnified.start();
                } else {
                    mGetUser.start();
                    mInitAAD.start();   // get refresh token for future AAD queries
                }
            }
        });

        mCalendarActivity = registerActivity(CalendarActivity.class, new ActivityHandler());
    }

    public void onConnectButtonClick(View v) {
        String clientId = Manager.Instance.getClientId();
        String redirectUri = Manager.Instance.getRedirectUri();

        if (Utils.isNullOrEmpty(clientId) || Utils.isNullOrEmpty(redirectUri)) {
            new AlertDialog.Builder(this)
                    .setTitle(getString(R.string.alert_title))
                    .setMessage(getString(R.string.app_needs_registration))
                    .setPositiveButton(getString(android.R.string.ok), null)
                    .show();
            return;
        }

        openHttpLog();
        mLoginActivity.start();

        mBtnConnect.setEnabled(false);
        mBtnConnect.setText(R.string.connecting);
    }

    private static void openHttpLog() {
        Manager.Instance.openHttpLog();
    }

    private void initViews(){
        mBtnConnect = (Button) findViewById(R.id.connectButton);
    }
}
