using System;
using Newtonsoft.Json;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class Performance
    {
        [JsonProperty("PerformanceId")]
        public int Id { get; set; }

        [JsonProperty("PerformanceDate")]
        public DateTime Date { get; set; }

        public int EventId { get; set; }

        [JsonProperty("ContainsDiscountOfferTickets")]
        public bool ContainsDiscountOfferTickets { get; set; }

        [JsonProperty("ContainsNoFeeOfferTickets")]
        public bool ContainsNoFeeOfferTickets { get; set; }
    }
}