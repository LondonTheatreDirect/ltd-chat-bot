using System;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Clients;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs.Intents
{
    // This intent shows if we have any tickets for specified event and date. Time is optional. 
    // Lists prices for different areas and pricebands.
    // If there are more than one performance on given date, shows all of them.
    // Typical question is "Do you have any available seats for Lion King next Tuesday?"
    [Intent("Availability Query")]
    public class AvailabilityQuery
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var @event = dataStorage.Events.List.FirstOrDefault(e => e.Name == state.LastEventName);
            if (@event == null) return DialogHelper.EventRequiredResponse();

            DateTime desiredDate;
            if (state.LastDate.HasValue)
                desiredDate = state.LastDate.Value;
            else if (state.LastDatePeriodFrom.HasValue)
                desiredDate = state.LastDatePeriodFrom.Value;
            else
                return DialogHelper.DateRequiredResponse();

            var client = new EventClient();
            var eventInfo = client.GetAvailableTickets(@event.Id, desiredDate, desiredDate.AddDays(1), 1);

            if ((eventInfo?.AvailableEvent?.AvailablePerformanceInfos?.Count ?? 0) == 0)
                return DialogHelper.NoPerformanceOrTicketResponse(state, @event);

            var relevantInfos = eventInfo.AvailableEvent.AvailablePerformanceInfos
                .Where(i => i.AvailableTicketsBlockInfos != null && i.AvailableTicketsBlockInfos.Count != 0)
                .ToList();

            if (state.LastTimePeriodFrom.HasValue && state.LastTimePeriodTo.HasValue)
            {
                relevantInfos = relevantInfos.Where(p => state.LastTimePeriodFrom.Value <= p.PerformanceDate.TimeOfDay &&
                                                         state.LastTimePeriodTo.Value >= p.PerformanceDate.TimeOfDay).ToList();
            }

            if (!relevantInfos.Any())
                return DialogHelper.NoPerformanceOrTicketResponse(state, @event);

            var message = "";
            foreach (var relevantInfo in relevantInfos)
            {
                var blocksInfo = relevantInfo.AvailableTicketsBlockInfos.GroupBy(b => b.AreaName);
                var blockMessage = "";
                foreach (var block in blocksInfo)
                {
                    var tickets = block.OrderBy(t => t.SellingPrice);
                    var lowest = tickets.First().SellingPrice;
                    var highest = tickets.Last().SellingPrice;
                    if (lowest == highest)
                        blockMessage += $"In {block.Key} we have available tickets for £{lowest}\r\n";
                    else
                        blockMessage += $"In {block.Key} we have available tickets from £{lowest} to £{highest}\r\n";
                }

                message +=
                    $"Yes, for event {eventInfo.AvailableEvent.EventName} played on {FormatHelper.FormatShortDateAndTime(relevantInfo.PerformanceDate)}" +
                    $" we have tickets starting from £{relevantInfo.MinimumTicketPrice} \r\n\r\n {blockMessage} \r\n\r\n";
            }

            return DialogHelper.SimpleResponse(message);
        }
    }
}