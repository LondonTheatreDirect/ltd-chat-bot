using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class Event
    {
        [JsonProperty("EventId")]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [JsonProperty("MinimumAge")]
        public string ChildRestriction { get; set; }

        [JsonProperty("RunningTime")]
        public string Duration { get; set; }

        public string SmallImageUrl { get; set; }

        public int VenueId { get; set; }

        public Venue Venue { get; set; }

        public decimal CurrentPrice { get; set; }

        [JsonProperty("EventType")]
        public int TypeId { get; set; }

        public EventType Type { get; set; }

        public List<Performance> Performances { get; set; }
    }
}