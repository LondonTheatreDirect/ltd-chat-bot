using System.Collections.Generic;
using ApiAiSDK.Model;
using LTDBot.Data;
using LTDBot.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LTDBotTests.Helpers
{
    [TestClass]
    public class ParameterHelperTests
    {
        private DataStorage _dataStorage;
        private DataStorage DataStorage => SmartHelper.GetOrCreate(ref _dataStorage);


        [TestMethod]
        public void GetEventFromParametersTest()
        {
            var response = new AIResponse { Result = new Result { Parameters = new Dictionary<string, object>()} };

            Assert.IsTrue(ParameterHelper.GetEventFromParameters(response, DataStorage.Events.List) == null);

            response.Result.Parameters.Add("Event", "lion");
            Assert.IsTrue(ParameterHelper.GetEventFromParameters(response, DataStorage.Events.List).Name == "The Lion King");

            response.Result.Parameters["Event"] = "musical"; 
            response.Result.ResolvedQuery = "";
            Assert.IsTrue(ParameterHelper.GetEventFromParameters(response, DataStorage.Events.List) == null);

            response.Result.ResolvedQuery = "I want to book Matilda The Musical";
            Assert.IsTrue(ParameterHelper.GetEventFromParameters(response, DataStorage.Events.List).Name == "Matilda The Musical");

            response.Result.Parameters["Event"] = "NotEvent";
            Assert.IsTrue(ParameterHelper.GetEventFromParameters(response, DataStorage.Events.List) == null);
        }

        [TestMethod]
        public void GetVenueFromParametersTest()
        {
            var response = new AIResponse { Result = new Result { Parameters = new Dictionary<string, object>() } };

            var venue = ParameterHelper.GetVenueFromParameters(response, DataStorage.Venues.List);
            Assert.IsTrue(venue == null);

            response.Result.Parameters.Add("Venue", "Vaudeville");
            venue = ParameterHelper.GetVenueFromParameters(response, DataStorage.Venues.List);
            Assert.IsTrue(venue.Name == "Vaudeville Theatre");

            response.Result.Parameters["Venue"] = "Theatre";
            response.Result.ResolvedQuery = "";
            venue = ParameterHelper.GetVenueFromParameters(response, DataStorage.Venues.List);
            Assert.IsTrue(venue == null);

            response.Result.ResolvedQuery = "where is Adelphi Theatre";
            venue = ParameterHelper.GetVenueFromParameters(response, DataStorage.Venues.List);
            Assert.IsTrue(venue.Name == "Adelphi Theatre");

            response.Result.Parameters["Venue"] = "NotVenue";
            venue = ParameterHelper.GetVenueFromParameters(response, DataStorage.Venues.List);
            Assert.IsTrue(venue == null);
        }
    }
}