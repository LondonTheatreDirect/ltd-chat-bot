using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using LTDBot.Modules.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using static LTDBot.Helpers.SmartHelper;

namespace LTDBot.Dialogs
{
    [Serializable]
    public class LtdAiDialog : IDialog<object>
    {
        private static ApiAi ApiAi => new ApiAi(new AIConfiguration(ConfigurationManager.AppSettings.Get("ApiAiClientAccessToken"), SupportedLanguage.English));

        private DataStorage _dataStorage;
        protected DataStorage DataStorage => GetOrCreate(ref _dataStorage);

        private IntentResolver _intentResolver;
        protected IntentResolver IntentResolver => GetOrProvide(ref _intentResolver, CreateIntentResolver);
        private IntentResolver CreateIntentResolver() => new IntentResolver(DataStorage);

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var activity = (Activity)await item;
            var response = FindFinalResponse(ApiAi, activity);

            if (!string.IsNullOrEmpty(response.Result.Fulfillment.Speech))  // If there is any simple answer returned from api.ai, show it to the user
            {
                await context.PostAsync(response.Result.Fulfillment.Speech);
            }
            else
            {
                var answer = await IntentResolver.FindAnswer(response, DataStorage.ConversationStates.FirstOrDefault(c => c.ConversationId == activity.Conversation.Id));

                if (answer.Attachments == null || !answer.Attachments.Any())
                    await context.PostAsync(answer.Text);
                else
                    await PostWithAttachments(activity, answer);
            }
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// Sometimes api.ai sends a prompt about something we already have in parameters. Check the prompt for a duplicate (do not send same prompt multiple times!)
        /// then, if we have prompted information in the last state, send it to api.ai as if the user responded.
        /// Returns final response, it may or may not be a prompt.
        /// </summary>
        private AIResponse FindFinalResponse(ApiAi apiAi, IMessageActivity activity)
        {
            bool stop = false;
            AIResponse response;
            do
            {
                response = CallService(apiAi, activity);  // send it to api.ai and get response
                UpdateConversationState(response, activity); // save contexts and parameters, we will need them next time when we call CallService
                var prompt = TreatPrompts(response, activity);  // check if we don't have the same prompt as last time

                var state = DataStorage.ConversationStates.FirstOrDefault(c => c.ConversationId == activity.Conversation.Id);
                if (prompt.StartsWith("I can tell you") || prompt.StartsWith("Very popular events") || state == null) // This isn't a question. Corresponds to predefined strings on api.ai.
                {
                    response.Result.Fulfillment.Speech = prompt;
                    return response;
                }

                if (prompt.ToLower().Contains("date") && state.LastDate != null)
                {
                    activity.Text = $"{state.LastDate:dd-MM-yyyy}";
                }
                else if (prompt.ToLower().Contains("event") && !string.IsNullOrEmpty(state.LastEventName))
                {
                    activity.Text = state.LastEventName;
                }
                else if (prompt.ToLower().Contains("venue") && !string.IsNullOrEmpty(state.LastVenueName))
                {
                    activity.Text = state.LastVenueName;
                }
                else
                {
                    stop = true;
                    response.Result.Fulfillment.Speech = prompt;
                }
            } while (stop == false);

            return response;
        }

        // Create or update conversation state for this conversation. Save contexts, parameters, last used event, venue, date etc.
        private void UpdateConversationState(AIResponse response, IActivity activity)
        {
            var currentConversationState = DataStorage.ConversationStates.FirstOrDefault(c => c.ConversationId == activity.Conversation.Id);
            if (currentConversationState == null)
            {
                DataStorage.ConversationStates.Add(new ConversationState {ConversationId = activity.Conversation.Id});
                currentConversationState = DataStorage.ConversationStates.First(c => c.ConversationId == activity.Conversation.Id);
            }

            currentConversationState.Contexts = response.Result.Contexts?.Select(c => new Context { Lifespan = c.Lifespan, Name = c.Name }).ToList();
            currentConversationState.Parameters = response.Result.Parameters;

            ParameterHelper.FillStateFromParameters(response, currentConversationState, DataStorage);

            DataStorage.SaveConversationStates();
        }

        /// <summary>
        /// Api.ai may return in response.Result.Fulfillment.Speech some aditional question like "what event?" or any other predefined reaction.
        /// This method ensures we never get into infinite loop of asking one question again and again.
        /// </summary>
        private string TreatPrompts(AIResponse response, IActivity activity)
        {
            string prompt;
            var currentConversationState = DataStorage.ConversationStates.FirstOrDefault(c => c.ConversationId == activity.Conversation.Id);
            if (currentConversationState == null) return string.Empty; // this shouldn't happen

            if (string.IsNullOrEmpty(response.Result.Fulfillment.Speech) ||
                string.IsNullOrEmpty(currentConversationState.LastSpeech) ||
                currentConversationState.LastSpeech != response.Result.Fulfillment.Speech)
            {
                prompt = response.Result.Fulfillment.Speech;  // show this speech/prompt to user
                currentConversationState.LastSpeech = response.Result.Fulfillment.Speech;
            }
            else
            {
                // this is the second time api.ai returned same prompt. Do not return it, restart conversation
                currentConversationState.Clear();
                prompt = "Sorry, it seems I can't grasp what you mean. Can you please start again from the beginning?";
            }
            DataStorage.SaveConversationStates();
            return prompt;
        }

        private AIResponse CallService(ApiAi apiAi, IMessageActivity activity)
        {
            // use contexts and parameters from last call if there is anything. We need this for follow up questions like any tickets|wicked|today
            var requestExtras = new RequestExtras { Contexts = AiContextsFromConversationState(activity.Conversation.Id) };

            return apiAi.TextRequest(activity.Text, requestExtras);
        }

        // Prepare contexts for api.ai
        private List<AIContext> AiContextsFromConversationState(string conversationId)
        {
            var state = DataStorage.ConversationStates.FirstOrDefault(c => c.ConversationId == conversationId);
            var result = new List<AIContext>();
            if (state?.Contexts == null || !state.Contexts.Any()) return result;

            foreach (var context in state.Contexts)
            {
                var aiContext = new AIContext { Lifespan = context.Lifespan, Name = context.Name, Parameters = new Dictionary<string, string>() };

                foreach (var contextParameter in state.Parameters)
                    aiContext.Parameters.Add(contextParameter.Key, contextParameter.Value.ToString());

                result.Add(aiContext);
            }
            return result;
        }

        private async Task PostWithAttachments(Activity message, Response ourResponse)
        {
            var connector = new ConnectorClient(new Uri(message.ServiceUrl));

            var replyToConversation = message.CreateReply(ourResponse.Text);
            replyToConversation.Type = "message";
            replyToConversation.Recipient = message.From;
            replyToConversation.Attachments = ourResponse.Attachments;
            await connector.Conversations.SendToConversationAsync(replyToConversation);
        }
    }
}