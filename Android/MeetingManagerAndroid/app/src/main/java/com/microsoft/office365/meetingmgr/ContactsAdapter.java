/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.widget.ImageView;
import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.Contact;

import java.util.List;

/**
 * List adapter for Contacts when using Microsoft Graph API
 */
public class ContactsAdapter extends BaseListAdapter<Contact> {

    public ContactsAdapter(ListView view, List<Contact> list) {
        super(view, list, R.layout.contact_row);
    }

    @Override
    protected void setView(Contact contact) {
        ImageView image = (ImageView) mConvertView.findViewById(R.id.photo);

        if (contact.photo != null) {
            image.setImageBitmap(contact.photo);
        } else {
            // Show some default image
            image.setImageResource(R.drawable.outlook_small);
        }
        setText(R.id.displayName, contact.toString());
    }
}
