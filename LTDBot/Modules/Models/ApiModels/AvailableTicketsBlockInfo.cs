using System;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class AvailableTicketsBlockInfo
    {
        public string AreaName { get; set; }

        public decimal SellingPrice { get; set; }
    }
}