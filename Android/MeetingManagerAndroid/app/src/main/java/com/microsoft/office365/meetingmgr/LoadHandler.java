package com.microsoft.office365.meetingmgr;

import android.os.Bundle;

/**
 * Base for miscellaneous load handlers
 */
public abstract class LoadHandler<T> {
    Bundle mArgs;

    abstract HttpCallable onCreate();
    abstract void onFinished(T data);

    String getArg() {
        return getArg(Constants.ARG_NAME);
    }

    String getArg(String key) {
        return mArgs.getString(key);
    }

    <T> T getArg(String key, Class<T> clazz) {
        return Args.getArg(mArgs, key, clazz);
    }

    <T> T getArg(String key, GetClass getClass) {
        return Args.getArg(mArgs, key, getClass);
    }
}
