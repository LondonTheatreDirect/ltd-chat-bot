using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs.Intents
{
    // Return predefined phrases from api.ai, when bot is confused.
    [Intent("Default Fallback Intent")]
    public class DefaultFallback
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            return DialogHelper.SimpleResponse(response.Result.Fulfillment.Speech);
        }
    }
}