using System;
using System.Collections.Generic;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class AvailableEvent
    {
        public string EventName { get; set; }
        
        public int EventId { get; set; }
        
        public List<AvailablePerformance> AvailablePerformanceInfos { get; set; }
    }
}