using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs.Intents
{
    // This intent primarily delete conversation state. Last used event, date, intent etc.
    // Typical question is "I want to start again"
    [Intent("Reset")]
    public class Reset
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            state.Clear();
            dataStorage.SaveConversationStates();
            return DialogHelper.SimpleResponse("OK, I will forget everything and you can ask me again.");
        }
    }
}