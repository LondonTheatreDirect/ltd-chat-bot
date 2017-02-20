using System.Web;
using System.Web.Http;
using Autofac;
using LTDBot.Modules;
using Microsoft.Bot.Builder.Dialogs;

namespace LTDBot
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DefaultExceptionMessageOverrideModule());
            builder.Update(Conversation.Container);
        }
    }
}