using Daimler.Api.Luis;
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

        private LuisIntentRecognizer _luisRecognizer;

        public EchoBot(LuisIntentRecognizer luisRecognizer)
        {
            _luisRecognizer = luisRecognizer;
        }


        protected async override Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await base.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);

            await turnContext.SendActivityAsync("Merhaba. Size hangi konuda yardımcı olmamı istersiniz?");

        }

        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var inputText = turnContext.Activity.Text.Trim().ToLower();

            //TODO: LUIS e input texti gönder....
            var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);

            await turnContext.SendActivityAsync(luisResult.Intents.OrderBy(i => i.Value.Score).FirstOrDefault().Key);
        }
    }
}
