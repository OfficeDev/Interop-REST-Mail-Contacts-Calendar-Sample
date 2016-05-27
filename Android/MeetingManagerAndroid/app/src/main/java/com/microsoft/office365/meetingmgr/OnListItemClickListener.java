/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.view.View;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;

/**
 * Base for list item click listeners
 */
public abstract class OnListItemClickListener<T> implements OnItemClickListener {
    @Override
    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
        @SuppressWarnings("unchecked")
        T object = (T) parent.getAdapter().getItem(position);
        onClick(object);
    }

    public abstract void onClick(T object);
}
