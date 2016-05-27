/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

/**
 * Implementation of Loader interface used to perform Http operations
 */
import android.content.AsyncTaskLoader;
import android.content.Context;

public class HttpLoader<D> extends AsyncTaskLoader<D> {
    private final HttpCallable<D> mCallable;
    private final HttpHelper mHttpProxy = new HttpHelper();

    public HttpLoader(Context context, HttpCallable<D> callable) {
        super(context);
        mCallable = callable;
        WaitDialog.show(context);
    }

    public String getErrorMessage() {
        return mHttpProxy.getErrorMessage();
    }

    @Override
    public D loadInBackground() {
        if (mCallable != null) {
            try {
                return mCallable.call(mHttpProxy);
            } catch (Exception e) {
                ErrorLogger.log(e);
            }
        }
        return null;
    }

    @Override
    public void deliverResult(D data) {
        WaitDialog.hide();
        super.deliverResult(data);
    }
}
