using System.Collections.Generic;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiAi
{
    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }
    }
}