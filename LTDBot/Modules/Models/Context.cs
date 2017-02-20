using System;

namespace LTDBot.Modules.Models
{
    [Serializable]
    public class Context
    {
        public string Name { get; set; }
        public int? Lifespan { get; set; }
    }
}