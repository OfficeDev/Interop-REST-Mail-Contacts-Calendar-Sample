//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Meeting_Manager_Xamarin.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Meeting_Manager_Xamarin
{
    public static class DateTimeUtils
    {
        public static string BuildRecurrentDate(Meeting.EventRecurrence recurrence)
        {
            var pattern = recurrence.Pattern;

            string date = ResMan.GetString("OccursPart");

            switch (pattern.Type.ToLower())
            {
                case OData.Daily:
                    date += string.Format(ResMan.GetString("DailyPart"), pattern.Interval);
                    break;

                case OData.Weekly:
                    date += ResMan.GetString("EveryPart");
                    string dayFmt = ResMan.GetString("WeeklyPartFirst");

                    var localDayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
                    var daysOfWeek = pattern.DaysOfWeek;
                    int numDays = daysOfWeek.Count;

                    for (int i = 0; i < numDays; ++i)
                    {
                        if (i > 0)
                        {
                            dayFmt = (i == numDays - 1) ?
                                    ResMan.GetString("WeeklyPartLast") :
                                    ResMan.GetString("WeeklyPartNext");
                        }

                        int dayIndex = GetDayOfWeekIndexInternal(daysOfWeek[i]);
                        date += string.Format(dayFmt, localDayNames[dayIndex]);
                    }
                    break;

                case OData.AbsoluteMonthly:
                    date += string.Format(ResMan.GetString("AbsMonthlyPart"),
                                            pattern.DayOfMonth,
                                            pattern.Interval);
                    break;

                case OData.RelativeMonthly:
                    date += string.Format(ResMan.GetString("RelMonthlyPart"),
                                           ApiOrdinalToLocalOrdinal(pattern.Index),
                                           GetDayOfWeekLocalized(pattern.DaysOfWeek[0]),
                                           pattern.Interval);
                    break;

                case OData.AbsoluteYearly:
                    {
                        var localMonths = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;

                        date += string.Format(ResMan.GetString("AbsYearlyPart"),
                                                pattern.Interval,
                                                localMonths[pattern.Month - 1],
                                                pattern.DayOfMonth);
                    }
                    break;

                case OData.RelativeYearly:
                    {
                        var localMonths = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;

                        date += string.Format(ResMan.GetString("RelYearlyPart"),
                                                pattern.Interval,
                                                ApiOrdinalToLocalOrdinal(pattern.Index),
                                                GetDayOfWeekLocalized(pattern.DaysOfWeek[0]),
                                                localMonths[pattern.Month - 1]);
                    }
                    break;
            }

            date += string.Format(ResMan.GetString("EffectivePart"),
                                           ToDateOnlyString(recurrence.Range.StartDate));

            switch (recurrence.Range.Type.ToLower())
            {
                case OData.NoEnd:
                    break;
                case OData.EndDate:

                    date += string.Format(ResMan.GetString("UntilPart"),
                                           ToDateOnlyString(recurrence.Range.EndDate));
                    break;
                case OData.Numbered:
                    DateTimeOffset endDate = CalculateEndDate(recurrence);
                    date += string.Format(ResMan.GetString("UntilPart"),
                                           ToDateOnlyString(endDate));
                    break;
            }

            return date;
        }

        private static DateTimeOffset CalculateEndDate(Meeting.EventRecurrence recurrence)
        {
            DateTimeOffset date = DateTimeOffset.Parse(recurrence.Range.StartDate);

            int rangeValue = (recurrence.Range.NumberOfOccurrences - 1) * recurrence.Pattern.Interval;

            switch (recurrence.Pattern.Type.ToLower())
            {
                case OData.Daily:
                    date = date.AddDays(rangeValue);
                    break;

                case OData.Weekly:
                    int occurrences = recurrence.Range.NumberOfOccurrences / recurrence.Pattern.DaysOfWeek.Count;
                    date = date.AddDays(CalculateDayDiff(date, recurrence.Pattern.DaysOfWeek));
                    date = date.AddDays(7 * (occurrences - 1) * recurrence.Pattern.Interval);
                    break;

                case OData.AbsoluteMonthly:
                case OData.RelativeMonthly:
                    date = date.AddMonths(rangeValue);
                    break;

                case OData.AbsoluteYearly:
                case OData.RelativeYearly:
                    date = date.AddYears(rangeValue);
                    break;
            }

            return date;
        }

        private static int CalculateDayDiff(DateTimeOffset date, List<string> days)
        {
            string lastDay = days.Last();

            return GetDayOfWeekIndexInternal(lastDay) - (int)date.DayOfWeek;
        }

        private static string ToDateOnlyString(string dateString)
        {
            var date = DateTimeOffset.Parse(dateString);

            return ToDateOnlyString(date);
        }

        private static string ToDateOnlyString(DateTimeOffset date)
        {
            return string.Format("{0:dddd MMMM d yyyy}", date);
        }

        private static string ApiOrdinalToLocalOrdinal(string index)
        {
            switch (index.ToLower())
            {
                case OData.First: return ResMan.GetString("First");
                case OData.Second: return ResMan.GetString("Second");
                case OData.Third: return ResMan.GetString("Third");
                case OData.Fourth: return ResMan.GetString("Fourth");
                case OData.Last: return ResMan.GetString("Last");
            }
            return "???";
        }

        private static string GetDayOfWeekLocalized(string odataDayOfWeek)
        {
            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            switch (odataDayOfWeek.ToLower())
            {
                case OData.Sunday: return dateTimeFormat.GetDayName(DayOfWeek.Sunday);
                case OData.Monday: return dateTimeFormat.GetDayName(DayOfWeek.Monday);
                case OData.Tuesday: return dateTimeFormat.GetDayName(DayOfWeek.Tuesday);
                case OData.Wednesday: return dateTimeFormat.GetDayName(DayOfWeek.Wednesday);
                case OData.Thursday: return dateTimeFormat.GetDayName(DayOfWeek.Thursday);
                case OData.Friday: return dateTimeFormat.GetDayName(DayOfWeek.Friday);
                case OData.Saturday: return dateTimeFormat.GetDayName(DayOfWeek.Saturday);
            }
            return "???";
        }

        private static int GetDayOfWeekIndexInternal(string day)
        {
            switch (day.ToLower())
            {
                case OData.Sunday: return (int)DayOfWeek.Sunday;
                case OData.Monday: return (int)DayOfWeek.Monday;
                case OData.Tuesday: return (int)DayOfWeek.Tuesday;
                case OData.Wednesday: return (int)DayOfWeek.Wednesday;
                case OData.Thursday: return (int)DayOfWeek.Thursday;
                case OData.Friday: return (int)DayOfWeek.Friday;
                case OData.Saturday: return (int)DayOfWeek.Saturday;
            }
            return 0;
        }

        public static IEnumerable<string> GetDaysOfWeek()
        {
            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            yield return dateTimeFormat.GetDayName(DayOfWeek.Sunday);
            yield return dateTimeFormat.GetDayName(DayOfWeek.Monday);
            yield return dateTimeFormat.GetDayName(DayOfWeek.Tuesday);
            yield return dateTimeFormat.GetDayName(DayOfWeek.Wednesday);
            yield return dateTimeFormat.GetDayName(DayOfWeek.Thursday);
            yield return dateTimeFormat.GetDayName(DayOfWeek.Friday);
            yield return dateTimeFormat.GetDayName(DayOfWeek.Saturday);
        }

        public static string DateToApiString(this DateTime dateTime)
        {
            return string.Format("{0:yyyy-MM-dd}", dateTime);
        }

        public static string DateToFullApiUtcString(this DateTime dateTime)
        {
            return string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", dateTime);
        }

        public static DateTime ToLocalTime(this ZonedDateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime.DateTime, TimeZoneInfo.Local);
        }

        public static DateTime ToUtcTime(this DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTime(localDateTime, TimeZoneInfo.Utc);
        }
    }
}
