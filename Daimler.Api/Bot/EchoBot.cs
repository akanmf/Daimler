using Daimler.Api.Luis;
using Daimler.Api.States;
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
        private UserState _userStateAccesor;
        private ConversationState _convStateAccesor;


        public EchoBot(LuisIntentRecognizer luisRecognizer, UserState userStateAccesor, ConversationState convStateAccesor)
        {
            _luisRecognizer = luisRecognizer;
            _userStateAccesor = userStateAccesor;
            _convStateAccesor = convStateAccesor;
        }


        protected async override Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await base.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);

            var conversationStateAccessors = _convStateAccesor.CreateProperty<PwdResetConversationStates>(nameof(PwdResetConversationStates));
            var conversationState = await conversationStateAccessors.GetAsync(turnContext, () => new PwdResetConversationStates());
            conversationState = PwdResetConversationStates.Initial;

            await turnContext.SendActivityAsync("Merhaba. Size hangi konuda yardımcı olmamı istersiniz?");

        }

        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //Conversation state ' e erişmeye çalışıyoruz.
            var conversationStateAccessors = _convStateAccesor.CreateProperty<PwdResetConversationStates>(nameof(PwdResetConversationStates));
            var conversationState = await conversationStateAccessors.GetAsync(turnContext, () => new PwdResetConversationStates());

            //UserState' e ulaşmaya çalışıyoruz.
            var userStateAccessors = _userStateAccesor.CreateProperty<UserPasswordInfo>(nameof(UserPasswordInfo));
            var userState = await userStateAccessors.GetAsync(turnContext, () => new UserPasswordInfo());


            var inputText = turnContext.Activity.Text.Trim().ToLower();

            switch (conversationState)
            {
                case PwdResetConversationStates.Initial:   // Sadece initial state 'te iken luis sorgusu olacak
                    //TODO: LUIS e input texti gönder....
                    var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);

                    if (luisResult.Intents.OrderBy(i => i.Value.Score).FirstOrDefault().Key != "SifreReset")
                    {

                        if (userState.Counter > 2)
                        {
                            await turnContext.SendActivityAsync("Sadece şifre resetleme yapabiliyorum diyorum. Niye inat ediyosun");
                            userState.Counter = 0;
                        }
                        else
                        {
                            await turnContext.SendActivityAsync("Sadece şifre resetleme yapabilirim.");
                            userState.Counter++;
                        }

                        return;
                    }

                    conversationState = PwdResetConversationStates.GetInfo;
                    break;
                case PwdResetConversationStates.GetInfo:



                    break;
                default:
                    break;
            }



            await turnContext.SendActivityAsync("Şifre resetleme işine başlıyorum.");


        }

        public async override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _convStateAccesor.SaveChangesAsync(turnContext, true, cancellationToken);
            await _userStateAccesor.SaveChangesAsync(turnContext, true, cancellationToken);
        }
    }
}
