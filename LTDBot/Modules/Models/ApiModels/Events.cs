using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class Events
    {
        [JsonProperty("Events")]
        public List<Event> List { get; set; } = new List<Event>();
    }
}