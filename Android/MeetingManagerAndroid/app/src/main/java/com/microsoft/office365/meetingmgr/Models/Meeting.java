package com.microsoft.office365.meetingmgr.Models;

import android.text.Html;
import android.text.Spanned;
import android.widget.ListView;

import com.microsoft.office365.meetingmgr.AttendeesAdapter;
import com.microsoft.office365.meetingmgr.GetClass;
import com.microsoft.office365.meetingmgr.HttpHelper;
import com.microsoft.office365.meetingmgr.OData;
import com.microsoft.office365.meetingmgr.Utils;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

/**
 * Base representation of Meeting (aka Event)
 */
public abstract class Meeting {
    public String Id;
    public String Subject;
    public String SeriesMasterId;
    public String Type;
    public boolean IsAllDay;
    public boolean IsOrganizer;
    public boolean IsCancelled;
    public ResponseStatus ResponseStatus;
    public Recurrence Recurrence;
    public Organizer Organizer = new Organizer();

    public Body Body = new Body();
    public List<Attendee> Attendees = new ArrayList<>();

    @Override
    public String toString() {
        return Subject;
    }

    public static Meeting newInstance() {
        return HttpHelper.isUnified() ? new MeetingNew() : new MeetingOld();
    }

    private static class ClassDeducer implements GetClass {
        @Override
        public Class<?> get(String json) {
            return isUnified(json) ? MeetingNew.class : MeetingOld.class;
        }
    }

    private static boolean isUnified(String json) {
        return json.contains("start");
    }

    public static ClassDeducer getClassDeducer() {
        return new ClassDeducer();
    }

    public static Class<? extends Meeting> getClassToUse() {
        return HttpHelper.isUnified() ? MeetingNew.class : MeetingOld.class;
    }

    public abstract void setStart(String time);

    public abstract String getStart();

    public abstract void setEnd(String time);

    public abstract String getEnd();

    public abstract void setStartTimeZone(String timeZone);

    public abstract String getStartTimeZone();

    public abstract void setEndTimeZone(String timeZone);

    public abstract String getEndTimeZone();

    public abstract String formatRecurrenceStartDate(Date startDate);

    public abstract String getCreatedTime();

    public abstract String getCreatedTimePropertyName();

    public AttendeesAdapter bindAttendeeList(ListView view) {
        return new AttendeesAdapter(view, new ArrayList<>(Attendees), this);
    }

    public boolean isPartOfSeries() {
        return Type.equalsIgnoreCase(OData.OCCURRENCE) || Type.equalsIgnoreCase(OData.EXCEPTION);
    }

    public boolean hasAccepted() {
        if (IsOrganizer) {
            return true;
        }

        if (ResponseStatus != null && ResponseStatus.Response != null) {
            String response = ResponseStatus.Response;

            // Check if current user already accepted
            return response.equalsIgnoreCase(OData.ACCEPTED) ||
                    response.equalsIgnoreCase(OData.TENTATIVELY_ACCEPTED);
        }

        return false;
    }

    public Location Location = new Location();

    public static class Recurrence {
        public Pattern Pattern = new Pattern();
        public Range Range = new Range();
    }

    public static class Pattern {
        public String Type = OData.DAILY;
        public int Interval = 1;
        public int DayOfMonth = 1;
        public int Month = 1;
        public List<String> DaysOfWeek = new ArrayList<>();
        public String FirstDayOfWeek = OData.SUNDAY;
        public String Index = OData.FIRST;   // week index
    }

    public static class Range {
        public String Type = OData.NO_END;
        public String StartDate;
        public String EndDate;
        public int NumberOfOccurrences = 10;
    }

    public static class Organizer {
        public EmailAddress EmailAddress = new EmailAddress();

        @Override
        public String toString() {
            return EmailAddress.toString();
        }
    }

    public static class Body {
        public String Content = "";
        public String ContentType = "HTML";

        public Spanned getText() {
            String description = Utils.stripHtmlComments(Content);
            return Html.fromHtml(description);
        }
    }
}
