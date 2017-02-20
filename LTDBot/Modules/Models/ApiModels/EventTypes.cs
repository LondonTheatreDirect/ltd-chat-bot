using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class EventTypes
    {
        [JsonProperty("EventTypes")]
        public List<EventType> List { get; set; } = new List<EventType>();
    }
}