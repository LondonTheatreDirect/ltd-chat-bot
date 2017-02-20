using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;
using Microsoft.Bot.Connector;

namespace LTDBot.Dialogs.Intents
{
    // Give various information about selected venue. If api.ai doesn't recognize what infotype is wanted, returns rich answer and button with types of information.
    // Typical question is "What can you tell me about Lyceum?"
    [Intent("Venue Info")]
    public class GetVenueInfo
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var venue = dataStorage.Venues.List.FirstOrDefault(v => v.Name == state.LastVenueName);
            if (venue == null)
                return DialogHelper.SimpleResponse("Sorry, I couldn't recognize the venue");

            object infoType;
            if (!response.Result.Parameters.TryGetValue("VenueInfo", out infoType))
                infoType = string.Empty;

            switch ((string)infoType)
            {
                case "Info":
                    return DialogHelper.DescriptionResponse(venue.Name,
                        venue.Info,
                        $"https://www.londontheatredirect.com/venue/{venue.Id}/{DialogHelper.GenerateUrlSafeString(venue.Name)}.aspx",
                        response);

                case "Tube":
                    return DialogHelper.SimpleResponse(string.IsNullOrEmpty(venue.NearestTube)
                        ? $"I cannot provide nearest tube near {venue.Name}."
                        : $"Nearest Tube for {venue.Name} is {venue.NearestTube}.");

                case "Train":
                    return DialogHelper.SimpleResponse(string.IsNullOrEmpty(venue.Train)
                        ? $"I cannot provide information about nearest train for {venue.Name}."
                        : $"Nearest train for {venue.Name} is {venue.Train}.");

                case "Town":
                    return DialogHelper.SimpleResponse($"{venue.Name} is in {venue.City}.");

                case "Address":
                    return DialogHelper.SimpleResponse(string.IsNullOrEmpty(venue.Postcode)
                        ? $"{venue.Name} is at {venue.City}, {venue.Address}, {venue.Postcode}."
                        : $"{venue.Name} is at {venue.City}, {venue.Address}.");

                case "Telephone":
                    return DialogHelper.SimpleResponse(string.IsNullOrEmpty(venue.Telephone)
                        ? $"I cannot provide telephone number for {venue.Name}."
                        : $"Telephone for {venue.Name} is {venue.Telephone}.");

                default:
                    return CreateOptionsResponse(venue);
            }
        }

        private static Response CreateOptionsResponse(Venue venue)
        {
            var buttons = new List<CardAction>();
            if (!string.IsNullOrEmpty(venue.Info))
                buttons.Add(DialogHelper.CreateCardAction($"Show me info for {venue.Name}", "Info"));

            if (!string.IsNullOrEmpty(venue.NearestTube))
                buttons.Add(DialogHelper.CreateCardAction($"What is the nearest tube for {venue.Name}?", "Nearest Tube"));

            if (!string.IsNullOrEmpty(venue.Train))
                buttons.Add(DialogHelper.CreateCardAction($"What is the nearest train for {venue.Name}?", "Train"));

            if (!string.IsNullOrEmpty(venue.Telephone))
                buttons.Add(DialogHelper.CreateCardAction($"Show me telephone for {venue.Name}", "Telephone"));

            buttons.Add(DialogHelper.CreateCardAction($"In what town {venue.Name} is?", "Town"));
            buttons.Add(DialogHelper.CreateCardAction($"Show me address of {venue.Name}", "Address"));
            buttons.Add(DialogHelper.CreateCardAction("Nevermind", "None"));

            return new Response
            {
                Attachments = new List<Attachment>
                {
                    new ThumbnailCard
                    {
                        Title = venue.Name,
                        Subtitle = "Please choose",
                        Buttons = buttons
                    }.ToAttachment()
                },
                Text = $"What do you want to know about {venue.Name}?"
            };
        }
    }
}