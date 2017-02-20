using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Clients;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;
using Microsoft.Bot.Connector;

namespace LTDBot.Dialogs.Intents
{
    // Shows varios info about given event.  If api.ai does not decode what type of info user wants,
    // bot shows rich answer with image and buttons for every type of information he knows.
    // Typical question is "Is Dead Funny appropriate for children?" or  "What can you tell me about Wicked?"
    [Intent("Event Info")]
    public class GetBasicEventInfo
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var @event = dataStorage.Events.List.FirstOrDefault(e => e.Name == state.LastEventName);
            if (@event == null) return DialogHelper.EventRequiredResponse();

            object infoType;
            if (!response.Result.Parameters.TryGetValue("EventInfo", out infoType))
                infoType = string.Empty;

            switch ((string)infoType)
            {
                case "Description":
                    return DialogHelper.DescriptionResponse(@event.Name,
                        @event.Description,
                        $"https://www.londontheatredirect.com/{DialogHelper.GetEventTypeUrlFolder(@event.Type.Name)}/{@event.Id}/{DialogHelper.GenerateUrlSafeString(@event.Name)}-tickets.aspx",
                        response);

                case "Type":
                    return DialogHelper.SimpleResponse($"{@event.Name} is {@event.Type.Name}.");

                case "Duration":
                    return DialogHelper.SimpleResponse(string.IsNullOrEmpty(@event.Duration)
                        ? $"Sorry I Do not know how long {@event.Name} lasts."
                        : $"{@event.Name} lasts {@event.Duration}.");

                case "ChildRestriction":
                    return DialogHelper.SimpleResponse(string.IsNullOrEmpty(@event.ChildRestriction)
                        ? $"There is no child restriction for {@event.Name}."
                        : $"{@event.Name} has this child restriction: {@event.ChildRestriction}.");

                case "RunningTime":
                    return DialogHelper.SimpleResponse(@event.Performances.Any()
                        ? $"The closest {@event.Name} performance starts at {FormatHelper.FormatShortDateAndTime(@event.Performances.First().Date)}."
                        : $"Sorry there is no performance of {@event.Name} in the next month.");

                case "Discount":
                {
                    var discountedPerformances = @event.Performances.Where(p => p.ContainsDiscountOfferTickets || p.ContainsNoFeeOfferTickets).ToList();
                    if (!discountedPerformances.Any())
                        return DialogHelper.SimpleResponse($"{@event.Name} has no discounted performances, sorry.");

                    return DialogHelper.SimpleResponse($"I have found {discountedPerformances.Count} performances of {@event.Name} with discounted tickets or sold without fees. " +
                                                       $"First is {FormatHelper.FormatShortDateAndTime(discountedPerformances.First().Date)}. If you want to know exact prices, ask me about available tickets.");
                }
                case "CurrentPrice":
                {
                    var price = @event.CurrentPrice;
                    if (price == 0 && @event.Performances.Any())
                    {
                        var client = new EventClient();
                        var eventInfo = client.GetAvailableTickets(@event.Id,
                            @event.Performances.First().Date,
                            @event.Performances.First().Date.AddDays(1), 2);

                        price = eventInfo?.AvailableEvent?
                                    .AvailablePerformanceInfos?
                                    .FirstOrDefault()?
                                    .MinimumTicketPrice ?? 0;
                    }

                    return DialogHelper.SimpleResponse(price != 0 ? $"{@event.Name} has price £{price}." : $"Sorry I have no information about prices for {@event.Name}");
                }
                case "Place":
                    return DialogHelper.SimpleResponse(@event.Venue != null
                        ? $"{@event.Name} is happening at {@event.Venue.Name}."
                        : $"Sorry I don't know where {@event.Name} is.");
                case "Performances":
                    return CreateAnswerPerformances(response, @event, state);

                default:
                    return CreateOptionsResponse(@event);
            }
        }

        private static Response CreateOptionsResponse(Event @event)
        {
            var buttons = new List<CardAction>();
            if (!string.IsNullOrEmpty(@event.Description))
                buttons.Add(DialogHelper.CreateCardAction($"Show me description of {@event.Name}", "Description"));

            if (!string.IsNullOrEmpty(@event.Duration))
                buttons.Add(DialogHelper.CreateCardAction($"Show me duration of {@event.Name}", "Duration"));

            if (!string.IsNullOrEmpty(@event.ChildRestriction))
                buttons.Add(DialogHelper.CreateCardAction($"Show me child restrictions for {@event.Name}", "Child Restriction"));

            if (@event.Performances.Any())
                buttons.Add(DialogHelper.CreateCardAction($"Show me running time of {@event.Name}", "Running Time"));

            if (@event.CurrentPrice != 0)
                buttons.Add(DialogHelper.CreateCardAction($"Show me current price of {@event.Name}", "Current Price"));

            if (@event.Venue != null)
                buttons.Add(DialogHelper.CreateCardAction($"Where is {@event.Name} happening?", "Place"));

            buttons.Add(DialogHelper.CreateCardAction($"Show me what type {@event.Name} is", "Type"));
            buttons.Add(DialogHelper.CreateCardAction($"Show me discounts for {@event.Name}", "Discounts"));
            buttons.Add(DialogHelper.CreateCardAction($"Show me performances of {@event.Name}", "Performances"));
            buttons.Add(DialogHelper.CreateCardAction($"Show me reviews of {@event.Name}", "Reviews"));
            buttons.Add(DialogHelper.CreateCardAction("Nevermind", "None"));

            return new Response
            {
                Attachments = new List<Attachment>
                {
                    new ThumbnailCard
                    {
                        Title = @event.Name,
                        Subtitle = "Please choose",
                        Buttons = buttons,
                        Images = new List<CardImage>
                        {
                            new CardImage(@event.SmallImageUrl)
                        }
                    }.ToAttachment()
                },
                Text = $"What do you want to know about {@event.Name}?"
            };
        }

        private static Response CreateAnswerPerformances(AIResponse response, Event @event, ConversationState state)
        {
            var list = ParameterHelper.FilterPerformanceByDateAndTime(state, @event.Performances);

            if (list.Count == 0) return DialogHelper.SimpleResponse($"{@event.Name} has no performances.");
            if (list.Count < 5 || response.Result.ResolvedQuery.Contains("complete"))
            {
                var performances = new StringBuilder($"List of {@event.Name} performances:\r\n\r\n");
                foreach (var performance in list)
                    performances.Append(FormatHelper.FormatLongDate(performance.Date) + "\r\n\r\n");

                return DialogHelper.SimpleResponse(performances.ToString());
            }

            var message = new StringBuilder($"First 5 performances of {@event.Name}:\r\n\r\n");
            foreach (var performance in list.Take(5))
                message.Append(FormatHelper.FormatLongDate(performance.Date) + "\r\n\r\n");

            return DialogHelper.ShowAllResponse(message.ToString(), $"There are {list.Count} performances, do you want to see all of them?", @event.SmallImageUrl, @event.Name,
                $"Show me complete list of performances of {@event.Name} {FormatHelper.FormatFromToDateFromToTime(state)}",
                "Show me all performances");
        }
    }
}