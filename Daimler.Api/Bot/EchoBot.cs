using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Daimler.Api.Bot
{
    public class EchoBot : ActivityHandler
    {
        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var inputText = turnContext.Activity.Text.Trim().ToLower();

            await turnContext.SendActivityAsync("Echo:" + inputText);
        }
    }
}
