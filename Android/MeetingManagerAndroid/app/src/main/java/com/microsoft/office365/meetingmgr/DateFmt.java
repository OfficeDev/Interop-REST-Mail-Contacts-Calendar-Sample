package com.microsoft.office365.meetingmgr;

import android.annotation.SuppressLint;
import android.content.Context;

import com.microsoft.office365.meetingmgr.Models.Meeting;

import java.text.DateFormat;
import java.text.DateFormatSymbols;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.TimeZone;

/**
 * Utilities for working with Date/Time fields
 */
public class DateFmt {
    private static final String mApiDateFormat = "yyyy-MM-dd";
    private static final String mFullFormatTZ = "yyyy-MM-dd'T'HH:mm:ssZ";
    private static final String mFullFormatNoTZ = "yyyy-MM-dd'T'hh:mm:ss";

    private static DateFormat createInstance(String pattern) {
        return new SimpleDateFormat(pattern, Locale.US);
    }

    public static DateFormat instance(String pattern) {
        return createInstance(pattern);
    }

    public static DateFormat instanceUTC(String pattern) {
        DateFormat fmt = createInstance(pattern);
        fmt.setTimeZone(TimeZone.getTimeZone("UTC"));
        return fmt;
    }

    public static Date dateFromApiDateString(String dateString) {
        DateFormat sdf;
        char last = dateString.charAt(dateString.length() - 1);
        int timeIndex;

        if (last == 'Z' || last == 'z') {
            sdf = instanceUTC(mFullFormatNoTZ);

        } else if ((timeIndex = dateString.indexOf("T")) < 0) {
            sdf = instance(mApiDateFormat);

        } else {
            if (dateString.indexOf(timeIndex, '-') > 0 ||
                dateString.indexOf(timeIndex, '+') > 0) {
                // Assume we have time zone
                sdf = instance(mFullFormatTZ);
            } else {
                // Assume date in default time zone
                sdf = instance(mFullFormatNoTZ);
            }
        }

        try {
            return sdf.parse(dateString);
        } catch (ParseException e) {
            ErrorLogger.log(e);
        }

        return new Date(0);
    }

    /**
     * Convert separate DateTime and TimeZone strings into a combined representation
     * @param dateTime
     * @param timeZone
     * @return
     */
    public static String getApiDateTimeWithTZ(String dateTime, String timeZone) {
        DateFormat sdf = DateFmt.instance(mFullFormatNoTZ);
        sdf.setTimeZone(TimeZone.getTimeZone(timeZone));

        DateFormat sdfOut = DateFmt.instance(mFullFormatTZ);
        return convertDateTime(dateTime, sdf, sdfOut);
    }

    public static String getApiDateTime(String dateTime, String timeZone) {
        DateFormat sdf = DateFmt.instance(mFullFormatNoTZ);
        sdf.setTimeZone(TimeZone.getTimeZone(timeZone));

        DateFormat sdfOut = DateFmt.instanceUTC(mFullFormatNoTZ);

        return convertDateTime(dateTime, sdf, sdfOut);
    }

    private static String convertDateTime(String dateTime, DateFormat srcFormat, DateFormat dstFormat) {
        try {
            Date date = srcFormat.parse(dateTime);
            return dstFormat.format(date);
        } catch (ParseException e) {
            ErrorLogger.log(e);
            return "";
        }
    }

    public static Date toDate(String dateTime) {
        return dateFromApiDateString(dateTime);
    }

    public static String toDateString(String dateTime) {
        return instance("EEEE MMMM d yyyy").format(toDate(dateTime));
    }

    public static String toShortDateString(String dateTime) {
        return instance("MM-dd").format(toDate(dateTime));
    }

    public static String toDateOnlyString(String dateTime) {
        return toDateOnlyString(toDate(dateTime));
    }

    public static String toTimeString(String dateTime) {
        return toTimeString(toDate(dateTime));
    }

    public static String toDateWithDayOfWeekString(Date date) {
        return instance("EEEE MMM d yyyy").format(date);
    }

    public static String toDateOnlyString(Date date) {
        return instance("MMMM d yyyy").format(date);
    }

    public static String toTimeString(Date date) {
        return instance("h:mm a").format(date);
    }

    public static String toFullDateString(Date date) {
        return instance("yyyy-MM-dd hh:mm:ssZ").format(date);
    }

    public static String toApiUtcString(Date date) {
        return instanceUTC("yyyy-MM-dd'T'HH:mm:ss'Z'").format(date);
    }

    public static String toApiDateString(Date date) {
        return instance(mApiDateFormat).format(date);
    }

    @SuppressLint("StringFormatMatches")
    public static String buildRecurrentDate(Context ctx, Meeting.Recurrence recurrence) {
        String date = ctx.getString(R.string.occurs_part);

        switch (recurrence.Pattern.Type.toLowerCase()) {
            case OData.DAILY:
                date += String.format(ctx.getString(R.string.daily_part),
                                        recurrence.Pattern.Interval);
                break;

            case OData.WEEKLY:
                date += ctx.getString(R.string.every_part);
                DateFormatSymbols symbols = new DateFormatSymbols();
                List<String> daysOfWeek = recurrence.Pattern.DaysOfWeek;

                String[] localDayNames = symbols.getWeekdays();
                int numDays = daysOfWeek.size();
                String dayFmt = ctx.getString(R.string.weekly_part_first);

                for (int i = 0; i < numDays; ++i) {
                    if (i > 0) {
                        dayFmt = (i == numDays - 1) ?
                                ctx.getString(R.string.weekly_part_last) :
                                ctx.getString(R.string.weekly_part_next);
                    }

                    int dayIndex = getDayOfWeekIndexInternal(daysOfWeek.get(i));
                    date += String.format(dayFmt, localDayNames[dayIndex]);
                }
                break;

            case OData.ABSOLUTE_MONTHLY:
                date += String.format(ctx.getString(R.string.abs_monthly_part),
                                        recurrence.Pattern.DayOfMonth,
                                        recurrence.Pattern.Interval);
                break;

            case OData.RELATIVE_MONTHLY:
                date += String.format(ctx.getString(R.string.rel_monthly_part),
                                        apiIndexToLocalIndex(ctx, recurrence.Pattern.Index),
                                        recurrence.Pattern.DaysOfWeek.get(0),
                                        recurrence.Pattern.Interval);
                break;

            case OData.ABSOLUTE_YEARLY:
                date += String.format(ctx.getString(R.string.abs_yearly_part),
                                        recurrence.Pattern.Interval,
                                        apiMonthIndexToLocalName(recurrence.Pattern.Month),
                                        recurrence.Pattern.DayOfMonth);
                break;

            case OData.RELATIVE_YEARLY:
                date += String.format(ctx.getString(R.string.rel_yearly_part),
                                        recurrence.Pattern.Interval,
                                        apiIndexToLocalIndex(ctx, recurrence.Pattern.Index),
                                        recurrence.Pattern.DaysOfWeek.get(0),
                                        apiMonthIndexToLocalName(recurrence.Pattern.Month));
                break;
        }

        date += String.format(ctx.getString(R.string.effective_part),
                                       toDateOnlyString(recurrence.Range.StartDate));

        switch (recurrence.Range.Type.toLowerCase()) {
            case OData.NO_END:
                break;
            case OData.END_BY:
                date += String.format(ctx.getString(R.string.until_part),
                                       toDateOnlyString(recurrence.Range.EndDate));
                break;
            case OData.END_AFTER:
                Date endDate = calculateEndDate(recurrence);
                date += String.format(ctx.getString(R.string.until_part),
                                       toDateOnlyString(endDate));
                break;
        }
        return date;
    }

    private static Date calculateEndDate(Meeting.Recurrence recurrence) {
        Date date = dateFromApiDateString(recurrence.Range.StartDate);
        Calendar cal = Calendar.getInstance();
        cal.setTime(date);

        int rangeValue = (recurrence.Range.NumberOfOccurrences - 1) * recurrence.Pattern.Interval;

        switch (recurrence.Pattern.Type.toLowerCase()) {
            case OData.DAILY:
                cal.add(Calendar.DAY_OF_MONTH, rangeValue);
                break;
            case OData.WEEKLY:
                int occurrences = recurrence.Range.NumberOfOccurrences / recurrence.Pattern.DaysOfWeek.size();
                cal.add(Calendar.DAY_OF_MONTH, calculateDayDiff(cal, recurrence.Pattern.DaysOfWeek));
                cal.add(Calendar.WEEK_OF_YEAR, (occurrences - 1) * recurrence.Pattern.Interval);
                break;
            case OData.ABSOLUTE_MONTHLY:
            case OData.RELATIVE_MONTHLY:
                cal.add(Calendar.MONTH, rangeValue);
                break;
            case OData.ABSOLUTE_YEARLY:
            case OData.RELATIVE_YEARLY:
                cal.add(Calendar.YEAR, rangeValue);
                break;
        }

        return cal.getTime();
    }

    private static int calculateDayDiff(Calendar cal, List<String> days) {
        String lastDay = days.get(days.size() - 1);

        return getDayOfWeekIndexInternal(lastDay) - cal.get(Calendar.DAY_OF_WEEK);
    }

    private static String apiMonthIndexToLocalName(int monthIndex) {
        DateFormatSymbols symbols = new DateFormatSymbols();

        return symbols.getMonths()[monthIndex - 1];
    }

    private static String apiIndexToLocalIndex(Context context, String index) {
        switch (index.toLowerCase()) {
            case OData.FIRST:   return context.getString(R.string.first);
            case OData.SECOND:  return context.getString(R.string.second);
            case OData.THIRD:   return context.getString(R.string.third);
            case OData.FOURTH:  return context.getString(R.string.fourth);
            case OData.LAST:    return context.getString(R.string.last);
        }
        return "???";
    }

    public static String indexToApiDayOfWeek(int dayIndex) {
        DateFormatSymbols symbols = new DateFormatSymbols();
        if (Utils.isNullOrEmpty(symbols.getWeekdays()[0])) {
            ++dayIndex;
        }

        switch (dayIndex) {
            case Calendar.SUNDAY:       return OData.SUNDAY;
            case Calendar.MONDAY:       return OData.MONDAY;
            case Calendar.TUESDAY:      return OData.TUESDAY;
            case Calendar.WEDNESDAY:    return OData.WEDNESDAY;
            case Calendar.THURSDAY:     return OData.THURSDAY;
            case Calendar.FRIDAY:       return OData.FRIDAY;
            case Calendar.SATURDAY:     return OData.SATURDAY;
        }
        return "???";
    }

    private static int getDayOfWeekIndexInternal(String day) {
        switch (day.toLowerCase()) {
            case OData.SUNDAY:      return Calendar.SUNDAY;
            case OData.MONDAY:      return Calendar.MONDAY;
            case OData.TUESDAY:     return Calendar.TUESDAY;
            case OData.WEDNESDAY:   return Calendar.WEDNESDAY;
            case OData.THURSDAY:    return Calendar.THURSDAY;
            case OData.FRIDAY:      return Calendar.FRIDAY;
            case OData.SATURDAY:    return Calendar.SATURDAY;
        }
        return 0;
    }

    public static int indexToApiMonth(int monthIndex) {
        return monthIndex + 1;
    }

    public static int apiMonthToIndex(int monthIndex) {
        return monthIndex - 1;
    }

    public static int getDayOfWeekIndex(String day) {
        int index = getDayOfWeekIndexInternal(day);
        DateFormatSymbols symbols = new DateFormatSymbols();
        if (Utils.isNullOrEmpty(symbols.getWeekdays()[0])) {
            --index;
        }
        return index;
    }

    public static List<String> getLocalWeekDays() {
        DateFormatSymbols symbols = new DateFormatSymbols();
        List<String> daysOfWeekItems = new ArrayList<>();

        for (String day : symbols.getWeekdays()) {
            if (!Utils.isNullOrEmpty(day)) {
                daysOfWeekItems.add(day);
            }
        }
        return daysOfWeekItems;
    }

    public static List<String> getLocalMonths() {
        DateFormatSymbols symbols = new DateFormatSymbols();
        List<String> monthItems = new ArrayList<>();

        for (String month : symbols.getMonths()) {
            if (!Utils.isNullOrEmpty(month)) {
                monthItems.add(month);
            }
        }
        return monthItems;
   }
}
