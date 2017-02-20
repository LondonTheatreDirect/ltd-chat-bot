using System;

namespace LTDBot.Modules.Models
{
    /// <summary>
    ///     ApiAi Intent Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntentAttribute : Attribute
    {
        public IntentAttribute(string intentName)
        {
            IntentName = intentName;
        }

        public string IntentName { get; set; }
    }
}