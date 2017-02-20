using System;
using System.Globalization;
using LTDBot.Modules.Models;

namespace LTDBot.Helpers
{
    public static class FormatHelper
    {
        public static string FormatLongDate(DateTime date)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var result = $"{date:dddd d}{GetDaySuffix(date)} {date:MMMM yyyy} {date.TimeOfDay}";
            return result;
        }

        public static string FormatShortDate(DateTime date)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            return $"{date:dd/MM/yyyy}";
        }

        public static string FormatShortDateAndTime(DateTime date)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            return $"{date:dd/MM/yyyy} {date.ToShortTimeString()}";
        }

        // returns in format 15/10/2016 or 15/10/2016, 12:00 or 15/10/2016, 12:00 - 16:00 
        public static string FormatDateFromToTime(ConversationState state)
        {
            if (state.LastDate != null)
            {
                return $"{FormatShortDate(state.LastDate.Value)}"
                       + (state.LastTimePeriodFrom.HasValue ? $", {state.LastTimePeriodFrom.Value}" : "")
                       + (state.LastTimePeriodTo == state.LastTimePeriodFrom || !state.LastTimePeriodTo.HasValue ? "" : $" - {state.LastTimePeriodTo.Value}");
            }
            return (state.LastTimePeriodFrom.HasValue ? $", {state.LastTimePeriodFrom.Value}" : "")
                   + (state.LastTimePeriodTo == state.LastTimePeriodFrom || !state.LastTimePeriodTo.HasValue ? "" : $" - {state.LastTimePeriodTo.Value}");
        }

        // returns in format 15/10/2016 or 15/10/2016 - 20/10/2016, 12:00 or 15/10/2016, 12:00 - 16:00  or 12:00 - 16:00
        public static string FormatFromToDateFromToTime(DateTime fromDate, DateTime toDate, TimeSpan fromTime, TimeSpan toTime)
        {
            string result = "";
            if (fromDate != new DateTime())
            {
                result += FormatShortDate(fromDate);

                if (toDate != fromDate)
                    result += $" - {FormatShortDate(toDate)}";
            }

            if (fromTime == new TimeSpan())
                return result;

            if (fromDate != new DateTime())
                result += ", ";

            result += fromTime.ToString();

            if (toTime != fromTime)
                result += $" - {toTime}";

            return result;
        }

        public static string FormatFromToDateFromToTime(ConversationState state)
        {
            return FormatFromToDateFromToTime(state.LastDatePeriodFrom ?? new DateTime(),
                state.LastDatePeriodTo ?? new DateTime(),
                state.LastTimePeriodFrom ?? new TimeSpan(),
                state.LastTimePeriodTo ?? new TimeSpan());
        }

        public static string GetDaySuffix(DateTime dateTime)
        {
            return GetDaySuffix(dateTime.Day);
        }

        public static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static string PluralizeEventTypeName(string typeName)
        {
            switch (typeName)
            {
                case "Comedy":
                    return "Comedies";
                case "Concerts":
                    return "Concerts";
                case "Experiences":
                    return "Experiences";
                case "Tennis":
                    return "Tennis";
                default:
                    return $"{typeName}s";
            }
        }
    }
}