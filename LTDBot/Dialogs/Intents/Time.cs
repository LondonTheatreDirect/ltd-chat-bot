using System;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs.Intents
{
    // Simple intent to show current time.
    [Intent("Time")]
    public class Time
    {
        public async Task<Response> Process(AIResponse response, ConversationState state, DataStorage dataStorage)
        {
            return DialogHelper.SimpleResponse($"Currently is {DateTime.Now.ToShortTimeString()}");
        }
    }
}