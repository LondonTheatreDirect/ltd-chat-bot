using System;
using System.Collections.Generic;

namespace LTDBot.Modules.Models
{
    [Serializable]
    public class ConversationState
    {
        public string ConversationId { get; set; }
        public List<Context> Contexts { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string LastSpeech { get; set; }
        public string LastEventName { get; set; }
        public DateTime? LastDate { get; set; }
        public string LastVenueName { get; set; }
        public string LastIntent { get; set; }
        public TimeSpan? LastTimePeriodFrom { get; set; }
        public TimeSpan? LastTimePeriodTo { get; set; }
        public DateTime? LastDatePeriodFrom { get; set; }
        public DateTime? LastDatePeriodTo { get; set; }

        public void Clear()
        {
            LastSpeech = string.Empty;
            Contexts?.Clear();
            Parameters?.Clear();
            LastEventName = string.Empty;
            LastDate = null;
            LastVenueName = string.Empty;
            LastIntent = string.Empty;
            LastTimePeriodFrom = null;
            LastTimePeriodTo = null;
            LastDatePeriodFrom = null;
            LastDatePeriodTo = null;
        }
    }
}