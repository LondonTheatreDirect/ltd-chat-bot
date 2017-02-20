using System;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class Review
    {
        public string ConsumerName { get; set; }
        public int Stars { get; set; }
        public string Content { get; set; }
    }
}