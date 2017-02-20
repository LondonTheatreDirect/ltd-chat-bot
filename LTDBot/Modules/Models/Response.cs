using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace LTDBot.Modules.Models
{
    public class Response
    {
        public string Text { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}