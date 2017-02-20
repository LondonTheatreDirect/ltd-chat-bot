using System;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs.Intents
{
    // Shows list of events sold without fees and events with discounts. Optionally you can specific date, date period or time period.
    // Typical question is "Are there any discounted events this week?"
    [Intent("Get Discounts")]
    public class GetDiscounts
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var performancesWithoutFees = dataStorage.Events.List.Where(e => e.Performances != null).SelectMany(e => e.Performances)
                .Where(p => p.ContainsNoFeeOfferTickets);

            var performancesWithDiscount = dataStorage.Events.List.Where(e => e.Performances != null).SelectMany(e => e.Performances)
                .Where(p => p.ContainsDiscountOfferTickets);

            if (state.LastDatePeriodFrom.HasValue)
            {
                performancesWithoutFees = ParameterHelper.FilterPerformanceByDateAndTime(state, performancesWithoutFees.ToList());
                performancesWithDiscount = ParameterHelper.FilterPerformanceByDateAndTime(state, performancesWithDiscount.ToList());
            }
            else // dateperiod is not set, just take next week
            {
                performancesWithoutFees = performancesWithoutFees.Where(p => p.Date < DateTime.Now.AddDays(7)).ToList();
                performancesWithDiscount = performancesWithDiscount.Where(p => p.Date < DateTime.Now.AddDays(7)).ToList();
            }

            var groupedWithoutFees = performancesWithoutFees.GroupBy(p => dataStorage.Events.List.First(e => e.Id == p.EventId).Name).ToList();
            var groupedWithDiscount = performancesWithDiscount.GroupBy(p => dataStorage.Events.List.First(e => e.Id == p.EventId).Name).ToList();
            if (!groupedWithoutFees.Any() && !groupedWithDiscount.Any())
                return DialogHelper.SimpleResponse("Sorry, I have no discounts");

            var result = "Yes we have some offers ";
            result += state.LastDatePeriodFrom.HasValue ? $"{FormatHelper.FormatFromToDateFromToTime(state)}\r\n\r\n" : "in next seven days\r\n\r\n";

            if (groupedWithoutFees.Any())
                result += $"These without fees: {string.Join(", ", groupedWithoutFees.Select(g => g.Key))}\r\n\r\n";

            if (groupedWithDiscount.Any())
                result += $"These events are discounted: {string.Join(", ", groupedWithDiscount.Select(g => g.Key))}\r\n\r\n";

            result += "If you want to see current prices, ask me about available tickets for particular event.";
            return DialogHelper.SimpleResponse(result);
        }
    }
}