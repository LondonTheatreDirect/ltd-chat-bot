using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Logger;
using LTDBot.Modules.Models;

namespace LTDBot.Dialogs
{
    [Serializable]
    public class IntentResolver
    {
        private readonly DataStorage _dataStorage;

        public IntentResolver(DataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        /// <param name="response">AIResponse what we got from api.ai.</param>
        /// <param name="state">State of conversation</param>
        public async Task<Response> FindAnswer(AIResponse response, ConversationState state)
        {
            try
            {
                if (state == null) state = new ConversationState();  // shouldn't happen

                var intentName = response.Result.Metadata.IntentName;
                if (intentName  == "Fill date" && !string.IsNullOrEmpty(state.LastIntent))
                    intentName = state.LastIntent;

                state.LastIntent = intentName;

                var intent = FindIntent(intentName);

                if (intent != null)
                {
                    dynamic intentClass = Activator.CreateInstance(intent);
                    var messageBack = (Task<Response>) intentClass.Process(response, state, _dataStorage);
                    messageBack.Wait();

                    if (!string.IsNullOrEmpty(messageBack.Result?.Text))
                        return messageBack.Result;
                }
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log($"Exception while finding answer: {e.Message}");
            }

            return DialogHelper.SimpleResponse("Sorry, I don't understand");
        }

        /// <summary>
        /// Determine which intent will be executed.
        /// </summary>
        /// <param name="intentName"> Name of intent to find. </param>
        /// <returns> Information about type, which will be solving intent. </returns>
        private static Type FindIntent(string intentName)
        {
            intentName = intentName?.Trim() ?? "Default Fallback Intent";

            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(m =>
                {
                    var attribute = m.GetCustomAttributes(typeof(IntentAttribute), false) as IntentAttribute[];
                    return attribute != null && attribute.Any(a => a.IntentName.Trim().Equals(intentName, StringComparison.OrdinalIgnoreCase));
                });
        }
    }
}