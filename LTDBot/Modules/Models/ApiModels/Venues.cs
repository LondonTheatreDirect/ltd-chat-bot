using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class Venues
    {
        [JsonProperty("Venues")]
        public List<Venue> List { get; set; } = new List<Venue>();
    }
}