using System;
using System.Collections.Generic;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class AvailablePerformance
    {
        public int PerformanceId { get; set; }

        public DateTime PerformanceDate { get; set; }

        public decimal MinimumTicketPrice { get; set; }

        public List<AvailableTicketsBlockInfo> AvailableTicketsBlockInfos { get; set; }
    }
}