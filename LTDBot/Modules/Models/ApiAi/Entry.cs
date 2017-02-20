using System.Collections.Generic;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiAi
{
    public class Entry
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("synonyms")]
        public List<string> Synonyms { get; set; }
    }
}