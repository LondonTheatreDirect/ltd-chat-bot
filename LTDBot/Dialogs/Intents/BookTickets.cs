using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;
using Microsoft.Bot.Connector;

namespace LTDBot.Dialogs.Intents
{
    // This intent creates rich answer with image, link to booking page according to specified event and date (optionally time)
    // If there are more than one performance, shows card with buttons with options.
    // Typical question is "Please get me four tickets for the Lion King on thursday"
    [Intent("Book tickets")]
    public class BookTickets
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var @event = dataStorage.Events.List.FirstOrDefault(e => e.Name == state.LastEventName);
            if (@event == null) return DialogHelper.EventRequiredResponse();

            if (state.LastDate == null) return DialogHelper.DateRequiredResponse();
            var desiredDate = state.LastDate.Value;

            var performances = @event.Performances?.Where(p => p.Date.Date == desiredDate.Date).ToList();

            if (performances != null && state.LastTimePeriodFrom.HasValue && state.LastTimePeriodTo.HasValue)
            {
                performances = performances.Where(p => state.LastTimePeriodFrom.Value <= p.Date.TimeOfDay
                                                       && state.LastTimePeriodTo.Value >= p.Date.TimeOfDay).ToList();
            }

            if (performances == null || !performances.Any())
                return DialogHelper.NoPerformanceOrTicketResponse(state, @event);

            var cardImages = new List<CardImage>
            {
                new CardImage(@event.SmallImageUrl)
            };

            if (performances.Count > 1)
            {
                var cardButtons = performances.Select(performance =>
                    DialogHelper.CreateCardAction($"I want to book tickets for {@event.Name} {FormatHelper.FormatShortDateAndTime(performance.Date)}", performance.Date.ToShortTimeString())).ToList();

                cardButtons.Add(DialogHelper.CreateCardAction("Nevermind", "None"));

                var plCard = new ThumbnailCard
                {
                    Title = @event.Name,
                    Subtitle = "What do you want?",
                    Images = cardImages,
                    Buttons = cardButtons
                };
                var plAttachment = plCard.ToAttachment();

                return new Response
                {
                    Attachments = new List<Attachment> { plAttachment },
                    Text = "There are multiple performances on that day!"
                };
            }
            else
            {
                var performance = performances.First();

                var cardButtons = new List<CardAction>();
                var plButton = new CardAction
                {
                    Value = DialogHelper.GetBooking2Url(performance.Id, performance.Date, @event.Name, @event.Venue.Name),
                    Type = "openUrl",
                    Title = "Book now"
                };
                cardButtons.Add(plButton);
                var plCard = new ThumbnailCard
                {
                    Title = @event.Name,
                    Subtitle = $"{FormatHelper.FormatLongDate(performance.Date)}",
                    Images = cardImages,
                    Buttons = cardButtons
                };
                var plAttachment = plCard.ToAttachment();

                return new Response { Attachments = new List<Attachment> { plAttachment }, Text = "Here it is" };
            }
        }
    }
}