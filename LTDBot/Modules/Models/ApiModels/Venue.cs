using System;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class Venue
    {
        [JsonProperty("VenueId")]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string NearestTube { get; set; }
        public string Train { get; set; }
    }
}