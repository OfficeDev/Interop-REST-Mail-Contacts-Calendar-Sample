/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.MailMessage;

import java.util.List;

/**
 * Recipients list adapter
 */
public class RecipientsAdapter extends BaseListAdapter<MailMessage.Recipient> {

    public RecipientsAdapter(ListView view, List<MailMessage.Recipient> data) {
        super(view, data, android.R.layout.simple_list_item_1);
        view.setAdapter(this);
    }

    public void add(MailMessage.Recipient object) {
        for (int i = 0; i < getCount(); ++i) {
            if (getItem(i).EmailAddress.Address.equalsIgnoreCase(object.EmailAddress.Address)) {
                return;
            }
        }
        super.add(object);
    }

    @Override
    protected void setView(MailMessage.Recipient object) {
        setText(android.R.id.text1, object.toString());
    }
}