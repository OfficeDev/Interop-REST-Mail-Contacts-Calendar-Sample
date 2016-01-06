package com.microsoft.office365.meetingmgr;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.List;


/**
 * Base class for miscellaneous list adapters
 */
abstract public class BaseListAdapter<T> extends ArrayAdapter<T> {
    private final int mResId;
    protected View mConvertView;

    public BaseListAdapter(ListView view, List<T> objects, int resId) {
        super(view.getContext(), 0, objects);

        mResId = resId;
        view.setAdapter(this);
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {

        LayoutInflater inflater = LayoutInflater.from(getContext());

        if (convertView == null) {
            convertView = inflater.inflate(mResId, parent, false);
        }

        mConvertView = convertView;
        setView(getItem(position));

        return convertView;
    }

    public List<T> getList() {
        ArrayList<T> result = new ArrayList<>();

        for (int i = 0; i < getCount(); ++i) {
            result.add(getItem(i));
        }
        return result;
    }

    protected void setText(int id, String text) {
        ((TextView) mConvertView.findViewById(id)).setText(text);
    }

    abstract protected void setView(T object);
}
