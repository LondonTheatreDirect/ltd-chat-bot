using System;

using LTDBot.Modules.Models.Descriptions;

namespace LTDBot.Modules.Models.ApiModels
{
    [Serializable]
    public class EventInfoCase
    {
        public Type ModelClass { get; set; }
        public Description Description { get; set; }
        public string EntityParent { get; set; }

        public dynamic Model  {get;set;}

        public Func<Event, dynamic> Selector { get; set; }

        public EventInfoCase(Type modelClass, Description description, string entityParent, Func<Event, dynamic> selector )
        {
            ModelClass = modelClass;
            Description = description;
            EntityParent = entityParent;
            Selector = selector;
        }
    }
}