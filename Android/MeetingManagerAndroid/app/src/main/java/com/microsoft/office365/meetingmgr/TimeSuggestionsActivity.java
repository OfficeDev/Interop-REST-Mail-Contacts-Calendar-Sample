package com.microsoft.office365.meetingmgr;

import android.os.Bundle;
import android.widget.ListView;

import com.microsoft.office365.meetingmgr.Models.Meeting;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidate;
import com.microsoft.office365.meetingmgr.Models.MeetingTimeCandidates;

/**
 * Displays free time slots
 *
 * Uses the following REST APIs:
 * 1. Find meeting times
 */
public class TimeSuggestionsActivity extends BaseActivity {
    private ListView mlvSuggestions;
    private Meeting mMeeting;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_time_suggestions);

        mMeeting = getArg(Meeting.getClassDeducer());

        mlvSuggestions = (ListView) findViewById(R.id.suggestions);

        mlvSuggestions.setOnItemClickListener(new OnListItemClickListener<MeetingTimeCandidate>() {
            @Override
            public void onClick(MeetingTimeCandidate object) {
                finish(object);
            }
        });

        registerLoadHandler(new LoadHandler<MeetingTimeCandidates>() {
            @Override
            public HttpCallable onCreate() {
                return new HttpCallable() {
                    @Override
                    public MeetingTimeCandidates call(HttpHelper hp) throws Exception {
                        TimeSuggestionsQuery tsq = new TimeSuggestionsQuery(mMeeting);
                        return tsq.getTimeCandidates(hp);
                    }
                };
            }

            @Override
            public void onFinished(MeetingTimeCandidates list) {
                new TimeslotAdapter(mlvSuggestions, list.value);
            }
        }).start();
    }
}
