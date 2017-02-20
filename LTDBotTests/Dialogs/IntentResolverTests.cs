using System;
using System.Collections.Generic;
using System.Linq;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Dialogs;
using LTDBot.Helpers;
using LTDBot.Modules.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static LTDBot.Helpers.SmartHelper;

namespace LTDBotTests.Dialogs
{
    [TestClass]
    public class IntentResolverTests
    {
        private DataStorage _dataStorage;
        private DataStorage DataStorage => GetOrCreate(ref _dataStorage);

        private IntentResolver _intentResolver;
        private IntentResolver IntentResolver => GetOrProvide(ref _intentResolver, CreateIntentResolver);
        private IntentResolver CreateIntentResolver() => new IntentResolver(DataStorage);

        public IntentResolverTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        [TestMethod]
        public void InvalidIntentTest()
        {
            var response = GetSimpleResponse("InvalidIntent");
            var answer = IntentResolver.FindAnswer(response, new ConversationState());
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Sorry, I don't understand");
        }

        [TestMethod]
        public void PromptTest()
        {
            var response = GetSimpleResponse("Book tickets");
            var state = new ConversationState();

            response.Result.ResolvedQuery = string.Empty;
            var answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Sorry, I couldn't recognize event");

            response.Result.Parameters.Add("Event", "The Lion King");
            response.Result.Parameters.Add("date", "definitelyNotADate");
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "When?");
        }

        [TestMethod]
        public void GetFilteredPerformances()
        {
            var response = GetSimpleResponse("Get Filtered Performances");
            response.Result.ResolvedQuery = "whats on 2017-03-15?";
            var state = new ConversationState();

            response.Result.Parameters.Add("date", "2017-03-15");  // far from ideal, we need to change date here every month to keep this test working
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            var answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("Let me show you 5 events on 15/03/2017:\r\n\r\nEvent The Lion King starting 14:30:00 and 19:30:00"));

            response.Result.Parameters.Add("time", "matinee");
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("Let me show you 5 events on 15/03/2017, 08:00:00 - 17:00:00:\r\n\r\nEvent The Lion King starting 14:30:00"));

            response.Result.Parameters["Venue"] = "Lyceum Theatre";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("Let me show you 1 event at Lyceum Theatre on 15/03/2017, 08:00:00 - 17:00:00:\r\n\r\nEvent The Lion King starting 14:30:00\r\n\r\n"));
             
            response.Result.Parameters["Venue"] = "Garrick Theatre";
            response.Result.Parameters["date"] = "2016-03-15";  // intentionally in the past
            response.Result.Parameters.Add("EventType", "Play");
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("Sorry, I have no Plays at Garrick Theatre on 15/03/2016, 08:00:00 - 17:00:00"));

            response.Result.Metadata.IntentName = "Reset";
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("OK, I will forget everything and you can ask me again."));

            response.Result.Metadata.IntentName = "Get Filtered Performances";
            response.Result.Parameters["Venue"] = string.Empty;
            response.Result.Parameters["date"] = "2017-03-15";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("Let me show you 5 Plays on 15/03/2017, 08:00:00 - 17:00:00:\r\n\r\nEvent"));
        }

        [TestMethod]
        public void GetVenueInfo()
        {
            var response = GetSimpleResponse("Venue Info");
            var state = new ConversationState();
            response.Result.Parameters.Add("Venue", "Adelphi Theatre");
            response.Result.Parameters.Add("VenueInfo", "Tube");
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            var answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Nearest Tube for Adelphi Theatre is Charing Cross/Embankment.");

            response.Result.Parameters["VenueInfo"] = "Info";
            response.Result.ResolvedQuery = "";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);        
            answer.Wait();
            Assert.IsTrue(answer.Result.Text.StartsWith("THE ADELPHI THEATRE, LONDON"));

            response.Result.Parameters["VenueInfo"] = "Town";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Adelphi Theatre is in London.");

            response.Result.Parameters["VenueInfo"] = "Train";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Nearest train for Adelphi Theatre is Charing Cross.");

            response.Result.Parameters["VenueInfo"] = "Address";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Adelphi Theatre is at London, Strand.");

            response.Result.Parameters["VenueInfo"] = "Telephone";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Telephone for Adelphi Theatre is 02037257069.");
        }

        [TestMethod]
        public void BookTickets()
        {
            var response = GetSimpleResponse("Book Tickets");
            response.Result.Parameters.Add("Event", "Matilda The Musical");
            response.Result.Parameters.Add("date", "2017-03-15");
            var state = new ConversationState();
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            var answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "There are multiple performances on that day!");
             
            response.Result.Parameters.Add("time", "12:00:00"); // it would be really nice test somehow api.ai decoding of evening/morning
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Sorry, there is no performance of Matilda The Musical on 15/03/2017, 12:00:00 or we have no tickets left");

            response.Result.Parameters["time"] = "matinee";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Here it is");

            response.Result.Parameters["time"] = "14:30:00";
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Here it is");
            Assert.IsTrue(answer.Result.Attachments != null && answer.Result.Attachments.Count == 1 && answer.Result.Attachments.First().Content != null);
            var content = (Microsoft.Bot.Connector.ThumbnailCard)answer.Result.Attachments.First().Content;
            Assert.AreEqual(content.Subtitle, "Wednesday 15th March 2017 14:30:00");

            response.Result.Parameters["time"] = "8:00:00/12:00:00"; // morning
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Sorry, there is no performance of Matilda The Musical on 15/03/2017, 08:00:00 - 12:00:00 or we have no tickets left");

            response.Result.Parameters["time"] = "12:00:00/16:00:00"; // afternoon
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Here it is");
            Assert.IsTrue(answer.Result.Attachments != null && answer.Result.Attachments.Count == 1 && answer.Result.Attachments.First().Content != null);
            content = (Microsoft.Bot.Connector.ThumbnailCard)answer.Result.Attachments.First().Content;
            Assert.AreEqual(content.Subtitle, "Wednesday 15th March 2017 14:30:00");

            response.Result.Parameters["time"] = "16:00:00/20:00:00"; // evening
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Here it is");
            Assert.IsTrue(answer.Result.Attachments != null && answer.Result.Attachments.Count == 1 && answer.Result.Attachments.First().Content != null);
            content = (Microsoft.Bot.Connector.ThumbnailCard)answer.Result.Attachments.First().Content;
            Assert.AreEqual(content.Subtitle, "Wednesday 15th March 2017 19:30:00");

            response.Result.Parameters["time"] = "matinee";
            response.Result.Parameters["date"]  = "2016-10-23"; // intentionaly in the past
            ParameterHelper.FillStateFromParameters(response, state, DataStorage);
            answer = IntentResolver.FindAnswer(response, state);
            answer.Wait();
            Assert.AreEqual(answer.Result.Text, "Sorry, there is no performance of Matilda The Musical on 23/10/2016, 08:00:00 - 17:00:00 or we have no tickets left");
        }

        private static AIResponse GetSimpleResponse(string intentName)
        {       
            return new AIResponse
            {
                Result = new Result
                {
                    Metadata = new Metadata { IntentName = intentName },
                    Fulfillment = new Fulfillment { Speech = "" },
                    Parameters = new Dictionary<string, object>()
                }
            };
        }
    }
}