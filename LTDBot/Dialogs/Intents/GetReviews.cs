using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Clients;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs.Intents
{
    // Returns summary of reviews for given event.
    // Typical question is "What people say about Lion King?"
    [Intent("Get Reviews")]
    public class GetReviews
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var @event = dataStorage.Events.List.FirstOrDefault(e => e.Name == state.LastEventName);
            if (@event == null) return DialogHelper.EventRequiredResponse();

            var client = new EventClient();
            var reviews = client.GetReviews(@event.Id);
            if (reviews == null)
                return DialogHelper.SimpleResponse($"Sorry, I don't have any review about event {@event.Name}");

            var chosenReview = reviews.Reviews.OrderByDescending(r => r.Stars).ThenBy(r => r.Content.Length).First();

            return DialogHelper.SimpleResponse($"{@event.Name} has many reviews. Average rating is {reviews.AverageRating}.\r\n\r\n" +
                                               $" {chosenReview.ConsumerName} gave {chosenReview.Stars} stars and said \"{chosenReview.Content} \"");
        }
    }
}