using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LTDBot.Dialogs;
using LTDBot.Modules.Logger;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace LTDBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
                    await Conversation.SendAsync(activity, () => new LtdAiDialog());

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception e)
            {
                Logger.GetInstance().Log($"Error: {e}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}