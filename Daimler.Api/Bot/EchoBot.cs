using AdaptiveCards;
using Daimler.Api.Luis;
using Daimler.Api.States;
using Daimler.Api.Validators;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            conversationState.CurrentState = PdwResetStates.Initial;

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

            switch (conversationState.CurrentState)
            {
                case PdwResetStates.Initial:   // Sadece initial state 'te iken luis sorgusu olacak

                    var inputText = turnContext.Activity.Text.Trim().ToLower();
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

                    await turnContext.SendActivityAsync("Talebinizi gerçekleştirebilmemiz için aşağıdaki bilgileri doldurunuz.");

                    Activity t = new Activity();
                    t.Type = ActivityTypes.Message;
                    t.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson() };
                    await turnContext.SendActivityAsync(t, cancellationToken);

                    conversationState.CurrentState = PdwResetStates.GetInfo;

                    break;
                case PdwResetStates.GetInfo:

                    userState = JsonConvert.DeserializeObject<UserPasswordInfo>(turnContext.Activity.Value.ToString());

                    // Kullanıcı adı validasyonu
                    if (!UserNameValidator.Validate(userState.UserName))
                    {
                        await turnContext.SendActivityAsync("Kullanıcı adınızı hatalı girdiniz. Lütfen bilgileri tekrar giriniz");
                        return;
                    }
                    
                    await turnContext.SendActivityAsync($"{userState.UserName} için şifreniz resetlenecektir. Onaylıyor musunuz?");

                    conversationState.CurrentState = PdwResetStates.GetApproval;

                    break;
                case PdwResetStates.GetApproval:

                    var approveText = turnContext.Activity.Text.Trim().ToLower();

                    if (approveText=="evet")
                    {
                        await turnContext.SendActivityAsync($"{userState.UserName} talebiniz alınmıştır");
                        conversationState.CurrentState = PdwResetStates.Completed;
                        return;
                    }
                    else if (approveText=="hayır")
                    {
                        await turnContext.SendActivityAsync($"{userState.UserName} talebiniz iptal edilmiştir.");
                        conversationState.CurrentState = PdwResetStates.Completed;
                        return;
                    }

                    await turnContext.SendActivityAsync($"Sizi anlayamadım");

                    break;
                case PdwResetStates.Completed:
                    await turnContext.SendActivityAsync($"İşleminiz tamamlanmıştır.");
                    await turnContext.SendActivityAsync("Size başka bir konuda yardımcı olmamı istersiniz?");
                    conversationState.CurrentState = PdwResetStates.Initial;
                    break;

            }

        }

        public async override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _convStateAccesor.SaveChangesAsync(turnContext, true, cancellationToken);
            await _userStateAccesor.SaveChangesAsync(turnContext, true, cancellationToken);
        }



        private Attachment CreateAdaptiveCardUsingJson()
        {
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = AdaptiveCard.FromJson(File.ReadAllText("AdaptiveCards\\AdaptiveCard.json")).Card
            };
        }
    }
}
