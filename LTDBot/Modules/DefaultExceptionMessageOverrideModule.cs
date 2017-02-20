using Autofac;
using Microsoft.Bot.Builder.Base;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace LTDBot.Modules
{
    // Register our own module to be able catch exception and never show it to user
    public class DefaultExceptionMessageOverrideModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c =>
                {
                    var cc = c.Resolve<IComponentContext>();

                    IPostToBot outer = new PersistentDialogTask(cc.Resolve<IEventLoop>, c.Resolve<IEventProducer<IActivity>>(), cc.Resolve<IBotData>());
                    outer = new PostUnhandledExceptionToUserOverrideTask(outer, cc.Resolve<IBotToUser>());
                    return outer;
                })
                .As<IPostToBot>()
                .InstancePerLifetimeScope();
        }
    }
}