package com.microsoft.office365.meetingmgr;

import android.widget.ArrayAdapter;
import android.widget.ListView;

import java.util.List;

/**
 *  List adapter for lists with a single item field
 */
public class SimpleAdapter<T>  extends ArrayAdapter<T> {
    public SimpleAdapter(ListView view, List<T> data) {
        super(view.getContext(), android.R.layout.simple_list_item_1, data);
        view.setAdapter(this);
    }
}
