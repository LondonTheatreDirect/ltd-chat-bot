using System;
using System.Collections.Generic;
using System.Linq;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;

namespace LTDBot.Helpers
{
    public static class ParameterHelper
    {
        public static List<Performance> FilterPerformanceByDateAndTime(ConversationState state, List<Performance> performances)
        {
            if (state.LastDatePeriodFrom.HasValue && state.LastDatePeriodTo.HasValue)
                performances = performances.Where(p => state.LastDatePeriodFrom.Value.Date <= p.Date.Date && state.LastDatePeriodTo.Value.Date.Date >= p.Date.Date).ToList();

            if (state.LastTimePeriodFrom.HasValue && state.LastTimePeriodTo.HasValue)
                performances = performances.Where(p => state.LastTimePeriodFrom.Value <= p.Date.TimeOfDay && state.LastTimePeriodTo.Value >= p.Date.TimeOfDay).ToList();

            return performances;
        }

        // Fills Conversation State with information from parameters. It does not rewrites field with null if parameters doesnt contain particular information.
        public static void FillStateFromParameters(AIResponse response, ConversationState currentConversationState, DataStorage dataStorage)
        {
            var @event = GetEventFromParameters(response, dataStorage.Events.List);
            currentConversationState.LastEventName = @event == null ? currentConversationState.LastEventName : @event.Name;

            var date = GetDateFromParameters(response);
            currentConversationState.LastDate = date ?? currentConversationState.LastDate;

            var venue = GetVenueFromParameters(response, dataStorage.Venues.List);
            currentConversationState.LastVenueName = venue == null ? currentConversationState.LastVenueName : venue.Name;

            TimeSpan timeFrom;
            TimeSpan timeTo;
            if (GetTimeFromParameters(response, out timeFrom, out timeTo))
            {
                currentConversationState.LastTimePeriodFrom = timeFrom;
                currentConversationState.LastTimePeriodTo = timeTo;
            }

            DateTime dateFrom;
            DateTime dateTo;
            if (GetDatesFromParameters(response, out dateFrom, out dateTo))
            {
                currentConversationState.LastDatePeriodFrom = dateFrom;
                currentConversationState.LastDatePeriodTo = dateTo;
            }
        }

        public static DateTime? GetDateFromParameters(AIResponse response)
        {
            object date;
            DateTime dateResult;
            if (!response.Result.Parameters.TryGetValue("date", out date))
                return null;

            if (date.ToString().ToLower().Trim() == "this day")
                return DateTime.Now;

            return !DateTime.TryParse(date.ToString(), out dateResult) ? (DateTime?)null : dateResult;
        }

        public static bool GetDatesFromParameters(AIResponse response, out DateTime fromDate, out DateTime toDate)
        {
            object date;
            object datePeriod;
            fromDate = toDate = new DateTime();
            response.Result.Parameters.TryGetValue("date", out date);
            response.Result.Parameters.TryGetValue("date-period", out datePeriod);

            if (string.IsNullOrEmpty(date?.ToString()) && string.IsNullOrEmpty(datePeriod?.ToString()))
                return false;

            if (date != null && date.ToString().ToLower().Trim() == "this day")
            {
                fromDate = toDate = DateTime.Now;
                return true;
            }

            if (date != null && DateTime.TryParse(date.ToString(), out fromDate))
            {
                toDate = fromDate;
                return true;
            }

            if (datePeriod == null)
                return false;

            var dateStrings = datePeriod.ToString().Split(new[] { "/" }, StringSplitOptions.None);
            return dateStrings.Length == 2 && DateTime.TryParse(dateStrings[0], out fromDate) && DateTime.TryParse(dateStrings[1], out toDate);
        }

        public static bool GetTimeFromParameters(AIResponse response, out TimeSpan fromTime, out TimeSpan toTime)
        {
            object time;
            fromTime = toTime = new TimeSpan();
            if (!response.Result.Parameters.TryGetValue("time", out time))
                return false;

            string timeString = time.ToString();
            if (timeString.ToLower().Trim().StartsWith("matinee"))
            {
                fromTime = TimeSpan.Parse("8:00");
                toTime = TimeSpan.Parse("17:00");
                return true;
            }

            if (timeString.Length < 10)  // only one time
            {
                if (!TimeSpan.TryParse(timeString, out fromTime)) return false;

                toTime = fromTime;
                return true;
            }

            var timeStrings = timeString.Split(new[] { "/", "-" }, StringSplitOptions.None);

            return timeStrings.Length == 2 && TimeSpan.TryParse(timeStrings[0], out fromTime) && TimeSpan.TryParse(timeStrings[1], out toTime);
        }

        public static Event GetEventFromParameters(AIResponse response, List<Event> events)
        {
            object eventName;
            if (!response.Result.Parameters.TryGetValue("Event", out eventName))
                return null;

            if (string.IsNullOrWhiteSpace(eventName.ToString()))
                return null;

            // sometimes api.ai return just part of event name "woman in" instead of "woman in black". This is not ideal, but better than nothing
            var potentialEvents = events.Where(v => v.Name.ToLower().Contains(eventName.ToString().ToLower())).ToList();
            if (potentialEvents.Count == 1)
                return potentialEvents.First();

            return potentialEvents.Count == 0 ? null : potentialEvents.FirstOrDefault(potentialEvent => response.Result.ResolvedQuery.ToLower().Contains(potentialEvent.Name.ToLower()));
        }

        public static Venue GetVenueFromParameters(AIResponse response, List<Venue> venues)
        {
            object venue;
            if (!response.Result.Parameters.TryGetValue("Venue", out venue))
                return null;

            string venueName = venue.ToString();
            if (string.IsNullOrWhiteSpace(venueName))
                return null;

            var potentialVenues = venues.Where(v => v.Name.ToLower().Contains(venueName.ToLower())).ToList();
            if (potentialVenues.Count == 1)
                return potentialVenues.First();

            return potentialVenues.Count == 0 ? null : potentialVenues.FirstOrDefault(potentialVenue => response.Result.ResolvedQuery.ToLower().Contains(potentialVenue.Name.ToLower()));
        }

        public static string GetTypeNameFromParameters(AIResponse response)
        {
            object type;
            return !response.Result.Parameters.TryGetValue("EventType", out type) ? "" : type.ToString();
        }
    }
}