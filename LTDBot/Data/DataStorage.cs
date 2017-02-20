using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using LTDBot.Modules.Clients;
using LTDBot.Modules.Models;
using LTDBot.Modules.Models.ApiModels;
using static LTDBot.Helpers.SmartHelper;

namespace LTDBot.Data
{
    [Serializable]
    public class DataStorage
    {
        private Events _events;
        public Events Events => GetOrProvide(ref _events, InitializeShows);

        private Venues _venues;
        public Venues Venues => GetOrProvide(ref _venues, InitializeVenues);

        private List<ConversationState> _conversationStates;
        public List<ConversationState> ConversationStates => GetOrProvide(ref _conversationStates, GetConversationStates);

        private List<ConversationState> GetConversationStates()
        {
            if (MemoryCache.Default.Contains("conversationStates"))
                return MemoryCache.Default["conversationStates"] as List<ConversationState>;

            return new List<ConversationState>();
        }

        public void SaveConversationStates()
        {
            MemoryCache.Default.Set("conversationStates", ConversationStates, GetCachePolicy());
        }

        private Events InitializeShows()
        {
            if (MemoryCache.Default.Contains("events"))
                return MemoryCache.Default["events"] as Events;

            var eventClient = new EventClient();
            var result = eventClient.AllEvents();
            var eventTypes = eventClient.AllTypes();

            foreach (var @event in result.List)
            {
                @event.Venue = Venues.List.FirstOrDefault(v => v.Id == @event.VenueId);
                @event.Type = eventTypes.List.FirstOrDefault(et => et.Id == @event.TypeId);
            }

            result.List = result.List.Where(e => e.Venue?.City?.ToLower() != "new york").ToList();  // We want only events in London.

            Parallel.ForEach(result.List, @event =>
            {
                @event.Performances = eventClient.GetEvent(@event.Id).Performances;
            });

            MemoryCache.Default.Set("events", result, GetCachePolicy());

            ApiAiEntityUpdater.UpdateEntities(result.List, Venues.List);

            return result;
        }

        private Venues InitializeVenues()
        {
            if (MemoryCache.Default.Contains("venues"))
                return MemoryCache.Default["venues"] as Venues;

            var venues = new VenueClient().All();

            MemoryCache.Default.Set("venues", venues, GetCachePolicy());

            return venues;
        }

        private static CacheItemPolicy GetCachePolicy()
        {
            return new CacheItemPolicy
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1))
            };
        }
    }
}