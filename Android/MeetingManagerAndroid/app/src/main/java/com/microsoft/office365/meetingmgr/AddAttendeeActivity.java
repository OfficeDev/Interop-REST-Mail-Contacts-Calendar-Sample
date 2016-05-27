/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
package com.microsoft.office365.meetingmgr;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.ADUser;
import com.microsoft.office365.meetingmgr.Models.Attendee;

import java.util.ArrayList;
import java.util.List;

/**
 * Displays a list of Active Directory users
 *
 * Uses the following REST APIs:
 * 1. Get users
 */
public class AddAttendeeActivity extends BaseActivity {
    private final static int PAGE_SIZE = 10;

    private EditText mEdtFilter;
    private Button mBtnApply;
    private Button mBtnNext;
    private Button mBtnPrev;
    private ListView mListUsers;

    private boolean mGetHumans;
    private boolean mBackwards;
    private UserPager mUserPager;
    private LoaderHolder mUserLoader;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_add_attendee);

        mGetHumans = Boolean.valueOf(getArg());

        setTitle(getString(mGetHumans ?
                        R.string.title_activity_select_attendee:
                        R.string.title_activity_select_room));

        mUserPager = new UserPager(PAGE_SIZE, mGetHumans);

        initViews();
        setEventListeners();
        registerLoaders();

        runAADQuery(true);
    }

    private void setEventListeners() {
        mListUsers.setOnItemClickListener(new OnListItemClickListener<ADUser>() {
            @Override
            public void onClick(ADUser user) {
                finishWithSelectedUser(user);
            }
        });

        mBtnApply.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                runAADQuery(true);
            }
        });

        mBtnNext.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                queryNextAADPage(false);
            }
        });

        mBtnPrev.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                queryNextAADPage(true);
            }
        });
    }

    private void queryNextAADPage(boolean backwards) {
        mBackwards = backwards;
        runAADQuery(false);
    }

    private void initViews() {
        mListUsers = (ListView) findViewById(R.id.users);
        mEdtFilter = (EditText) findViewById(R.id.filter);
        mBtnApply = (Button) findViewById(R.id.apply);
        mBtnNext = (Button) findViewById(R.id.next);
        mBtnPrev = (Button) findViewById(R.id.prev);
    }

    private void registerLoaders() {
        mUserLoader = registerLoadHandler(new LoadHandler<List<ADUser>>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public List<ADUser> call(HttpHelper hp) throws Exception {
                        return mUserPager.getNextPage(hp, mBackwards);
                    }
                };
            }

            @Override
            public void onFinished(List<ADUser> events) {
                new SimpleAdapter<>(mListUsers, events);

                mBtnNext.setEnabled(mUserPager.hasNextPage());
                mBtnPrev.setEnabled(mUserPager.hasPrevPage());
            }
        });
    }

    private void runAADQuery(boolean restart) {
        mBtnNext.setEnabled(false);
        mBtnPrev.setEnabled(false);

        if (restart) {
            mBackwards = false;
            final String filter = mEdtFilter.getText().toString();

            mUserPager.init(HttpHelper.isUnified() ? buildUnifiedUri(filter) : buildUri(filter));
        }
        mUserLoader.start();
    }

    private void finishWithSelectedUser(ADUser item) {
        finish(new Attendee(item));
    }

    private String buildUri(String filter) {
        String baseUri = Constants.AAD_ENDPOINT + "users?";

        StringBuilder sb = new StringBuilder(baseUri);
        buildFilter(sb, filter);

        sb.append("&api-version=1.5");

        return sb.toString();
    }

    private String buildUnifiedUri(String filter) {
        String baseUri = Constants.OFFICE_ENDPOINT_UNIFIED + "users?";

        StringBuilder sb = new StringBuilder(baseUri);
        buildFilter(sb, filter);

        return sb.toString();
    }

    private StringBuilder buildFilter(StringBuilder sb, String filter) {
        // For now, we don't have a way to filter just humans or rooms.
        if (!mGetHumans) {
            buildRoomsQuery(sb, filter);
        } else {
            buildHumansQuery(sb, filter);
        }

        if (sb.charAt(sb.length() - 1) != '?') {
            sb.append('&');
        }
        sb.append(String.format("$top=%s", PAGE_SIZE));

        return sb;
    }

    private void buildHumansQuery(StringBuilder sb, String filter) {
        if (!Utils.isNullOrEmpty(filter)) {
            sb.append("$filter=");
            addStartsWith(sb, filter, "givenName");
            sb.append(" or ");
            addMoreNameFilters(sb, filter);
        }
    }

    private void buildRoomsQuery(StringBuilder sb, String filter) {
        sb.append("$filter=");
        // For rooms, we are making assumption about their 'givenName' property.
        addStartsWith(sb, "Conf Room", "givenName");

        if (!Utils.isNullOrEmpty(filter)) {
            sb.append(" and ");
            sb.append('(');
            addMoreNameFilters(sb, filter);
            sb.append(')');
        }
    }

    private void addMoreNameFilters(StringBuilder sb, String filter) {
        addStartsWith(sb, filter, "userPrincipalName");
        sb.append(" or ");
        addStartsWith(sb, filter, "displayName");
    }

    private void addStartsWith(StringBuilder sb, String filter, String property) {
        sb.append(String.format("startswith(%s,'%s')", property, filter));
    }

    private static class UserPager {
        private String mFirstPageUri;
        private String mNextPageUri;
        private String mPrevPageUri;
        private int mCurPage = -1;
        private final int mPageSize;
        private final boolean mGetHumans;

        UserPager(int pageSize, boolean getHumans) {
            mPageSize = pageSize;
            mGetHumans = getHumans;
        }

        void init(String uri) {
            mFirstPageUri = mNextPageUri = uri;
            mPrevPageUri = null;
            mCurPage = -1;
        }

        boolean hasNextPage() {
            return mNextPageUri != null;
        }

        boolean hasPrevPage() {
            return mCurPage > 0;
        }

        List<ADUser> getNextPage(HttpHelper hp, boolean backwards) {
            List<ADUser> users = new ArrayList<>();

            if (backwards) {
                if (mCurPage <= 1) {
                    mNextPageUri = mFirstPageUri;
                } else if (mPrevPageUri != null && !mPrevPageUri.contains("previous-page")) {
                    mNextPageUri = mPrevPageUri + "&previous-page=true";
                }
            }

            while (mNextPageUri != null && users.size() < mPageSize) {

                HttpHelper.PagedResult<ADUser> res = hp.getPagedItems(mNextPageUri, ADUser.class);

                List<ADUser> list = res.list;
                if (list == null) {
                    break;
                }

                boolean someAdded = false;
                for (ADUser u : list) {
                    users.add(u);
                    someAdded = true;
                }

                if (backwards) {
                    --mCurPage;
                } else if (someAdded) {
                    ++mCurPage;
                }

                mNextPageUri = res.nextUri;

                if (mNextPageUri != null && mNextPageUri.contains("$skiptoken")) {
                    mPrevPageUri = mNextPageUri;
                }
            }

            if (users.isEmpty()) {
                ++mCurPage;     // correct count if the last page was empty
            }

            return users;
        }
    }
}
