using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace LTDBot.Modules
{
    public class PostUnhandledExceptionToUserOverrideTask : IPostToBot
    {
        private readonly IBotToUser _botToUser;
        private readonly IPostToBot _inner;

        public PostUnhandledExceptionToUserOverrideTask(IPostToBot inner, IBotToUser botToUser)
        {
            SetField.NotNull(out _inner, nameof(inner), inner);
            SetField.NotNull(out _botToUser, nameof(botToUser), botToUser);
        }

        public async Task PostAsync(IActivity item, CancellationToken token)
        {
            try
            {
                await _inner.PostAsync(item, token);
            }
            catch (Exception error)
            {
             /*   try    // uncomment for displaying exception
                {
                    await _botToUser.PostAsync($"Message: {error.Message} Exception: {error}", cancellationToken: token);
                }
                catch (Exception innerEx)
                {
                    await _botToUser.PostAsync($"Inner message: {innerEx.Message}", cancellationToken: token);
                }                */
            }
        }
    }
}