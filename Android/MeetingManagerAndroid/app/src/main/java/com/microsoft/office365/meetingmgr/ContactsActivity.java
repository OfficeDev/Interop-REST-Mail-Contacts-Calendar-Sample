/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.Attendee;
import com.microsoft.office365.meetingmgr.Models.Contact;

import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Displays Contacts list.
 *
 * Uses the following REST APIs:
 * 1. Get contacts' count
 * 2. Get contacts
 * 3. Get contact photo
 */

public class ContactsActivity extends BaseActivity {
    private final static int PAGE_SIZE = 10;

    private Button mBtnNext;
    private Button mBtnPrev;
    private ListView mListContacts;

    private int mContactsCount;
    private int mCurPageIndex;
    private LoaderHolder mContactsLoader;
    private LoaderHolder mGetCountLoader;
    private LoaderHolder mPhotosDataLoader;

    private ContactsAdapter mAdapter;
    private List<Contact> mCurrentContacts;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_contacts);

        initViews();
        setEventListeners();
        registerLoaders();

        mBtnNext.setEnabled(false);
        mBtnPrev.setEnabled(false);

        mGetCountLoader.start();
    }

    private void initViews() {
        mBtnNext = (Button) findViewById(R.id.next);
        mBtnPrev = (Button) findViewById(R.id.prev);
        mListContacts = (ListView) findViewById(R.id.contacts);
    }

    private void setEventListeners() {
        mListContacts.setOnItemClickListener(new OnListItemClickListener<Contact>() {
            @Override
            public void onClick(Contact object) {
                finishWithSelectedContact(object);
            }
        });

        mBtnNext.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ++mCurPageIndex;
                mContactsLoader.start();
            }
        });

        mBtnPrev.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                --mCurPageIndex;
                mContactsLoader.start();
            }
        });
    }

    private void registerLoaders() {
        mGetCountLoader = registerLoadHandler(new LoadHandler<Integer>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public Integer call(HttpHelper hp) throws Exception {
                        return hp.getItem("contacts/$count", Integer.class);
                    }
                };
            }

            @Override
            public void onFinished(Integer data) {
                mContactsCount = data;
                mContactsLoader.start();
            }
        });

        mContactsLoader = registerLoadHandler(new LoadHandler<List<Contact>>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public List<Contact> call(HttpHelper hp) throws Exception {
                        String uri = String.format("contacts?$top=%s&$skip=%s&$orderby=DisplayName", PAGE_SIZE, PAGE_SIZE*mCurPageIndex);

                        HttpHelper.PagedResult<Contact> res = hp.getPagedItems(uri, Contact.class);
                        return res.list;
                    }
                };
            }

            @Override
            public void onFinished(List<Contact> data) {

                mBtnPrev.setEnabled(mCurPageIndex > 0);
                mBtnNext.setEnabled(PAGE_SIZE * (mCurPageIndex + 1) < mContactsCount);

                if (HttpHelper.isUnified()) {
                    mCurrentContacts = data;
                    mAdapter = new ContactsAdapter(mListContacts, new ArrayList<Contact>());
                    mPhotosDataLoader.start();
                } else {
                    new SimpleAdapter<>(mListContacts, data);
                }
            }
        });

        mPhotosDataLoader = registerLoadHandler(new LoadHandler<Void>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public Void call(HttpHelper hp) throws Exception {

                        ExecutorService executor = Executors.newFixedThreadPool(4);
                        List<Callable<Void>> todoList = new ArrayList<>();

                        for (Contact contact : mCurrentContacts) {
                            todoList.add(createPhotoLoadCallable(contact));
                        }

                        executor.invokeAll(todoList);
                        return null;
                    }
                };
            }

            @Override
            public void onFinished(Void data) {
            }
        });
    }

    private Callable<Void> createPhotoLoadCallable(final Contact contact) {
        return new Callable<Void>() {
            @Override
            public Void call() throws Exception {
                HttpHelper hp = new PhotoHttpProxy();

                String uri = String.format("contacts/%s/photo/$value", contact.Id);
                byte[] bytes = hp.getItem(uri, byte[].class);

                if (bytes != null) {
                    contact.photo = BitmapFactory.decodeByteArray(bytes, 0, bytes.length);
                }

                Manager.Instance.postRunnable(new Runnable() {
                    @Override
                    public void run() {
                        mAdapter.add(contact);
                        mAdapter.sort(new Comparator<Contact>() {
                            @Override
                            public int compare(Contact lhs, Contact rhs) {
                                return lhs.toString().compareTo(rhs.toString());
                            }
                        });
                    }
                });
                return null;
            }
        };
    }

    private static class PhotoHttpProxy extends HttpHelper {
        @Override
        protected void handleFailure(int statusCode, String errMessage) {
            // Ignore "not found"
            if (statusCode != 404) {
                super.handleFailure(statusCode, errMessage);
            }
        }
    }

    private void finishWithSelectedContact(Contact item) {
        if (item.EmailAddresses.length <= 0) {
            finish();
        }

        Attendee attendee = new Attendee(item.EmailAddresses[0]);
        finish(attendee);
    }
}
