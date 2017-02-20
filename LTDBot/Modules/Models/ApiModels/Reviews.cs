using System;
using System.Collections.Generic;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class ReviewsSummary
    {
        public double AverageRating { get; set; }
        public List<Review> Reviews { get; set; }
    }
}