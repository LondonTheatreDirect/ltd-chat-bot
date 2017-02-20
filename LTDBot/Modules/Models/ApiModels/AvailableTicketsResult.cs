using System;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class AvailableTicketsResult
    {
        public AvailableEvent AvailableEvent { get; set; }
    }
}