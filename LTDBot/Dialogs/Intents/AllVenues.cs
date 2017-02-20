using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;

namespace LTDBot.Dialogs.Intents
{
    // Shows list of 5 venues and offers to show all of them. 
    // Typical question is "Show me all venues"
    [Intent("All Venues")]
    public class AllVenues
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            var venueList = dataStorage.Venues.List;
            if (venueList.Count == 0)
                return DialogHelper.SimpleResponse("No venues found");

            if (venueList.Count <= 5 || response.Result.ResolvedQuery.Contains("complete"))
                return DialogHelper.SimpleResponse(CreateAnswer(venueList));

            return DialogHelper.ShowAllResponse(CreateAnswer(venueList.Take(5).ToList()),
                $"I found {venueList.Count} venues, do you want to see all of them?",
                "", "Venues", $"{response.Result.ResolvedQuery} complete", "Show me all");
        }

        private static string CreateAnswer(IEnumerable<Venue> venueList)
        {
            return string.Join("\r\n\r\n", venueList.Select(e => e.Name));
        }
    }
}