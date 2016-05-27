/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.app.ActivityManager;
import android.app.AlertDialog;
import android.app.LoaderManager;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.Loader;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.ContextMenu;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.PopupMenu;
import android.widget.Spinner;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Base class for app activities
 */
public class BaseActivity  extends AppCompatActivity implements LoaderManager.LoaderCallbacks {
    private List<LoadHandler> mLoaders = new ArrayList<>();

    private Map<Integer, ActivityHandler> mActivities = new HashMap<>();

    private Map<View, ContextMenuHandler> mContextMenuHandlers = new HashMap<>();
    private AdapterView.AdapterContextMenuInfo mContextInfo;

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_calendar, menu);

        menu.findItem(R.id.action_settings).setVisible(!(this instanceof RegistrationActivity));
        menu.findItem(R.id.action_sendlog).setVisible(Manager.Instance.getUser() != null);

        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        switch (item.getItemId()) {

            case R.id.action_settings:
                Intent intent = new Intent(getApplicationContext(), RegistrationActivity.class);
                startActivity(intent);
                return true;
            case R.id.action_sendlog:
                Manager.Instance.getHttpTracer().sendLog();
                return true;
            case R.id.action_disconnect:
                cleanup();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    private void cleanup() {
        ((ActivityManager) this.getSystemService(Context.ACTIVITY_SERVICE)).clearApplicationUserData();
    }

    protected void bindDateField(TextView text, Calendar calendar, Runnable callback) {
        new DateSelector(getFragmentManager(), text, calendar, callback);
    }

    protected void bindDateField(TextView text, Calendar calendar) {
        new DateSelector(getFragmentManager(), text, calendar, null);
    }

    protected void bindTimeField(TextView text, Calendar calendar, Runnable callback) {
        new TimeSelector(getFragmentManager(), text, calendar, callback);
    }

    protected class LoaderHolder {
        final private int loaderId;

        LoaderHolder(int id) {
            loaderId = id;
        }

        protected void start() {
            start((ArgPair[])null);
        }

        protected void start(String value) {
            start(new ArgPair(Constants.ARG_NAME, value));
        }

        protected void start(ArgPair... args) {
            Bundle bundle = new Bundle();
            startLoad(loaderId, Args.putArgs(bundle, args));
        }
    }

    protected LoaderHolder registerLoadHandler(LoadHandler loader) {
        mLoaders.add(loader);
        return new LoaderHolder(mLoaders.size() - 1);
    }

    private void startLoad(int id, Bundle args) {
        getLoaderManager().restartLoader(id, args, this).forceLoad();
    }

    @Override
    public Loader onCreateLoader(int id, Bundle args) {
        LoadHandler handler = mLoaders.get(id);
        handler.mArgs = args;

        return new HttpLoader(this, handler.onCreate());
    }

    @Override
    public void onLoadFinished(Loader loader, Object data) {
        if (data == null) {
            HttpLoader httpLoader = (HttpLoader) loader;
            String errorMessage = httpLoader.getErrorMessage();

            if (errorMessage != null) {
                showAlert(errorMessage);
            }
            return;
        }

        LoadHandler handler = mLoaders.get(loader.getId());
        handler.onFinished(data);
    }

    @Override
    public void onLoaderReset(Loader loader) {
    }

    protected void showAlert(String message) {
        new AlertDialog.Builder(this)
                .setTitle(getString(R.string.alert_title))
                .setMessage(message)
                .setNeutralButton(getString(R.string.alert_caption_neutral), null)
                .show();
    }

    protected void getConfirmation(String message, DialogInterface.OnClickListener listener) {
        new AlertDialog.Builder(this)
                .setTitle(getString(R.string.alert_title))
                .setMessage(message)
                .setPositiveButton(getString(R.string.alert_caption_positive), listener)
                .setNegativeButton(getString(R.string.alert_caption_negative), listener)
                .show();
    }

    protected static class ActivityHandler {
        Intent mIntent;
        void onResult() {
        }
        void onCanceled() {
        }

        String getArg() {
            return getArg(Constants.ARG_NAME);
        }

        String getArg(String key) {
            return Args.getArg(mIntent, key);
        }

        <T> T getArg(Class<T> clazz) {
            return getArg(Constants.ARG_NAME, clazz);
        }

        <T> T getArg(String key, Class<T> clazz) {
            return Args.getArg(mIntent, key, clazz);
        }
    }

    protected final class ActivityHolder {
        private final Class<?> mClass;
        private final int mRequestCode;

        ActivityHolder(Class<?> clazz, int requestCode) {
            mClass = clazz;
            mRequestCode = requestCode;
        }

        protected void start() {
            start((Object) null);
        }

        protected void start(Object arg) {
            start(new ArgPair[]{new ArgPair(Constants.ARG_NAME, arg)});
        }

        protected final void start(ArgPair... args) {
            Intent intent = new Intent(getApplicationContext(), mClass);
            startActivityForResult(Args.putArgs(intent, args), mRequestCode);
        }
    }

    protected ActivityHolder registerActivity(Class<?> clazz) {
        return registerActivity(clazz, null);
    }

    protected ActivityHolder registerActivity(Class<?> clazz, ActivityHandler onResult) {
        int requestCode = mActivities.size();
        mActivities.put(requestCode, onResult);

        return new ActivityHolder(clazz, requestCode);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        ActivityHandler handler = mActivities.get(requestCode);

        if (handler != null) {
            if (resultCode == RESULT_CANCELED){
                handler.onCanceled();
            } else {
                handler.mIntent = data;
                handler.onResult();
            }
        }
    }

    protected static abstract class ContextMenuHandler<T> {
        private ContextMenu mContextMenu;
        private MenuInflater mInflater;

        void onCreate(T object) {}
        abstract boolean onItemSelected(int itemId, T object);

        protected void inflate(int resourceId) {
            mInflater.inflate(resourceId, mContextMenu);
        }

        protected MenuItem findItem(int resourceId) {
            return mContextMenu.findItem(resourceId);
        }
    }

    protected <T> void bindContextMenu(View view, final int rId, final ContextMenuHandler<T> handler) {
        mContextMenuHandlers.put(view, handler);

        view.setOnCreateContextMenuListener(new View.OnCreateContextMenuListener() {
            @Override
            public void onCreateContextMenu(ContextMenu menu, View v, ContextMenu.ContextMenuInfo menuInfo) {
                handler.mInflater = getMenuInflater();
                handler.mContextMenu = menu;

                AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo) menuInfo;
                AdapterView<?> parent = (AdapterView<?>) info.targetView.getParent();

                if (rId != 0) {
                    handler.inflate(rId);
                }

                @SuppressWarnings("unchecked")
                T object = (T) parent.getAdapter().getItem(info.position);
                handler.onCreate(object);
            }
        });
    }

    protected <T> void bindContextMenu(View view, final ContextMenuHandler<T> handler) {
        bindContextMenu(view, 0, handler);
    }

    @Override
    public boolean onContextItemSelected(MenuItem item) {
        AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo) item.getMenuInfo();

        if (info != null) {
            mContextInfo = info;
        }

        AdapterView<?> view = (AdapterView<?>) mContextInfo.targetView.getParent();
        ContextMenuHandler handler = mContextMenuHandlers.get(view);

        Object object = view.getAdapter().getItem(mContextInfo.position);

        boolean result = handler.onItemSelected(item.getItemId(), object);

        if (!result) {
            result = super.onContextItemSelected(item);
        }
        return result;
    }

    protected static abstract class PopupMenuHandler {
        private PopupMenu mPopup;

        void onCreate() {}
        abstract boolean onItemSelected(int itemId);

        protected void inflate(int resourceId) {
            mPopup.inflate(resourceId);
        }

        protected MenuItem findItem(int resourceId) {
            Menu popupMenu = mPopup.getMenu();
            return popupMenu.findItem(resourceId);
        }
    }

    protected void bindPopupMenu(final Button button, final int rId, final PopupMenuHandler handler) {
        button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                handler.mPopup = new PopupMenu(button.getContext(), v);

                if (rId != 0) {
                    handler.inflate(rId);
                }

                handler.onCreate();

                handler.mPopup.setOnMenuItemClickListener(new PopupMenu.OnMenuItemClickListener() {
                    @Override
                    public boolean onMenuItemClick(MenuItem item) {
                        return handler.onItemSelected(item.getItemId());
                    }
                });

                handler.mPopup.show();
            }
        });
    }

    protected void bindSimpleSpinner(Spinner spinner, List<String> items) {
        spinner.setAdapter(new ArrayAdapter<>(this, android.R.layout.simple_spinner_dropdown_item, items));
    }

    protected void bindSimpleSpinner(Spinner spinner, int resourceId) {
        spinner.setAdapter(ArrayAdapter.createFromResource(this, resourceId, android.R.layout.simple_spinner_dropdown_item));
        spinner.setSelection(0);    // default selection
    }

    protected void finish(ArgPair... args) {
        setResult(RESULT_OK, Args.putArgs(new Intent(), args));
        finish();
    }

    protected void finish(Object arg) {
        finish(new ArgPair[] {new ArgPair(Constants.ARG_NAME, arg)});
    }

    protected <T> T getArg(Class<T> clazz) {
        return getArg(Constants.ARG_NAME, clazz);
    }

    protected <T> T getArg(GetClass getClass) {
        return getArg(Constants.ARG_NAME, getClass);
    }

    protected <T> T getArg(String key, Class<T> clazz) {
        return Args.getArg(getIntent(), key, clazz);
    }

    protected <T> T getArg(String key, GetClass getClass) {
        return Args.getArg(getIntent(), key, getClass);
    }

    protected String getArg() {
        return getArg(Constants.ARG_NAME);
    }

    protected String getArg(String key) {
        return Args.getArg(getIntent(), key);
    }
}
