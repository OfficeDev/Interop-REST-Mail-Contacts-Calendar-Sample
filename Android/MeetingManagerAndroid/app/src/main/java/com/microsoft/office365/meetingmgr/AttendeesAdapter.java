package com.microsoft.office365.meetingmgr;

/**
 * List adapter for Meeting Attendee list
 */

import android.widget.ListView;
import com.microsoft.office365.meetingmgr.Models.Attendee;
import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.util.List;

public class AttendeesAdapter extends BaseListAdapter<Attendee> {
    private final String mOrganizerAddress;

    public AttendeesAdapter(ListView view, List<Attendee> list, Meeting meeting) {
        super(view, list, R.layout.attendee_row);
        view.setAdapter(this);

        mOrganizerAddress = meeting.Organizer.EmailAddress.Address;
    }

    public void add(Attendee object) {
        for (int i = 0; i < getCount(); ++i) {
            if (getItem(i).EmailAddress.Address.equalsIgnoreCase(object.EmailAddress.Address)) {
                return;
            }
        }
        super.add(object);
    }

    @Override
    public void setView(Attendee att) {
        String noResponse = getContext().getString(R.string.status_noResponse);
        String response = null;
        String required = null;

        if (att.EmailAddress.Address.equalsIgnoreCase(mOrganizerAddress)) {
            required = getContext().getString(R.string.status_organizer);
        } else {
            if (att.Status != null && att.Status.Response != null) {
                response = att.Status.Response.equalsIgnoreCase(OData.NONE) ?
                        noResponse : att.Status.Response;
            } else {
                response = noResponse;
            }

            if (att.Type != null) {
                required = att.Type.equalsIgnoreCase(OData.REQUIRED) ?
                        getContext().getString(R.string.attendee_required) :
                        getContext().getString(R.string.attendee_optional);
            }
        }

        setText(R.id.email, att.EmailAddress.toString());
        setText(R.id.required, required);
        setText(R.id.status, response);
    }
}
