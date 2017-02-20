using System;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class EventType
    {
        [JsonProperty("EventTypeId")]
        public int Id { get; set; }

        [JsonProperty("EventTypeName")]
        public string Name { get; set; }
    }
}