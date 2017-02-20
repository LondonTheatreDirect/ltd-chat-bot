using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;

namespace LTDBot.Dialogs.Intents
{
    // This intent can filter performances byt type, date, time and venues. If there are more than 5 performances, shows 5 and offers buttons to show all.
    // Typical question is "Do you have any musicals tomorrow evening?"
    [Intent("Get Filtered Performances")]
    public class GetFilteredPerformances
    {
        private string _typeName;
        private string _venueName;

        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            FillParametersFromResponse(response, state);

            var relevantPerformances = GetRelevantPerformances(dataStorage, state);  // get list of performances filtered by date, time, venue and type if specified

            if (!relevantPerformances.Any())
                return DialogHelper.SimpleResponse($"Sorry, I have no {FilterSpecification(true, state)}");

            var groupedPerformances = relevantPerformances.GroupBy(p => p.EventId);

            var performances = groupedPerformances as IList<IGrouping<int, Performance>> ?? groupedPerformances.ToList();
            if (performances.Count <= 5 || response.Result.ResolvedQuery.Contains("complete"))
                return DialogHelper.SimpleResponse(CreateAnswer(dataStorage, performances, false, state));

            return DialogHelper.ShowAllResponse(CreateAnswer(dataStorage, performances, true, state),
                $"I found {performances.Count} events and {relevantPerformances.Count} performances, do you want to see all of them?",
                "", "Events", $"Show me complete list of {FilterSpecification(true, state)}", "Show me all");
        }

        private string FilterSpecification(bool multiple, ConversationState state)
        {
            var typename = string.IsNullOrEmpty(_typeName) ? "event" : _typeName;

            var result = multiple ? FormatHelper.PluralizeEventTypeName(typename) : typename;
            result += string.IsNullOrEmpty(_venueName) ? "" : $" at {_venueName}";
            var dateSpecification = FormatHelper.FormatFromToDateFromToTime(state);
            if (!string.IsNullOrEmpty(dateSpecification))
                result += $" on {dateSpecification}";

            return result;
        }

        private string CreateAnswer(DataStorage dataStorage, IEnumerable<IGrouping<int, Performance>> grouped, bool firstFive, ConversationState state)
        {
            var eventIdPerformances = grouped as IList<IGrouping<int, Performance>> ?? grouped.ToList();
            var count = firstFive ? Math.Min(eventIdPerformances.Count, 5).ToString() : eventIdPerformances.Count.ToString();
            var message = new StringBuilder($"Let me show you {count} {FilterSpecification(eventIdPerformances.Count > 1, state)}:\r\n\r\n");

            grouped = firstFive ? eventIdPerformances.Take(5) : eventIdPerformances;
            foreach (var group in grouped)
            {
                if (state.LastDatePeriodFrom == state.LastDatePeriodTo && state.LastDatePeriodFrom.HasValue)
                {
                    message.Append($"Event {dataStorage.Events.List.First(e => e.Id == group.Key).Name} starting {string.Join(" and ", group.Select(g => g.Date.TimeOfDay))}\r\n\r\n");
                }
                else
                {
                    string s = group.Count() > 1 ? "s" : "";
                    message.Append($"Event {dataStorage.Events.List.First(e => e.Id == group.Key).Name} with {group.Count()} performance{s}.\r\n\r\n");
                }
            }

            return message.ToString();
        }

        private List<Performance> GetRelevantPerformances(DataStorage dataStorage, ConversationState state)
        {
            var performances = GetFilteredEvents(dataStorage)
                .Where(e => e.Performances != null)
                .SelectMany(e => e.Performances.Select(p => p))
                .ToList();

            return ParameterHelper.FilterPerformanceByDateAndTime(state, performances);
        }

        private void FillParametersFromResponse(AIResponse response, ConversationState state)
        {
            _typeName = ParameterHelper.GetTypeNameFromParameters(response);
            _venueName = state.LastVenueName ?? "";
        }

        private IEnumerable<Event> GetFilteredEvents(DataStorage dataStorage)
        {
            var eventList = dataStorage.Events.List;

            if (!string.IsNullOrEmpty(_venueName))
                eventList = eventList.Where(e => e.Venue.Name == _venueName).ToList();

            if (!string.IsNullOrEmpty(_typeName))
                eventList = eventList.Where(e => e.Type.Name == _typeName).ToList();

            return eventList;
        }
    }
}