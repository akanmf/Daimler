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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

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

            var _userCompleted = await userStateAccessors.GetAsync(turnContext, () => new UserPasswordInfo());

            switch (conversationState.CurrentState)
            {
                case PdwResetStates.Initial:   // Sadece initial state 'te iken luis sorgusu olacak
                    await InitialStateOperations(turnContext, conversationState, userState, cancellationToken);
                    break;
                case PdwResetStates.GetInfo:
                  var userFromAdaptiveCard = await GetInfoOperations(turnContext, conversationState, userState);

                    userState.Application = userFromAdaptiveCard.Application;
                    userState.UserName = userFromAdaptiveCard.UserName;
                    userState.Email = userFromAdaptiveCard.Email;
                    userState.Counter = userFromAdaptiveCard.Counter;

                    break;
                case PdwResetStates.GetApproval:
                    await GetApprovalOperations(turnContext, conversationState, userState);
                    break;
                case PdwResetStates.Completed:
                    await turnContext.SendActivityAsync($"İşleminiz tamamlanmıştır.");
                    await turnContext.SendActivityAsync("Size başka bir konuda yardımcı olmamı ister misiniz?");
                    conversationState.CurrentState = PdwResetStates.Initial;
                    break;

            }

        }

        public async Task GetApprovalOperations(ITurnContext<IMessageActivity> turnContext, PwdResetConversationStates conversationState, UserPasswordInfo userState)
        {
            var approveText = turnContext.Activity.Text.Trim().ToLower();

            // onaylıyorum, tamam gibi intentler luise eklenebilir.
            if (approveText == "evet")
            {
                await turnContext.SendActivityAsync($"Sayın {userState.UserName}, {userState.Application} uygulaması için şifre reset talebiniz alınmıştır.");
                conversationState.CurrentState = PdwResetStates.Completed;
                //Operations.ExcelCreator.Create(userState);

                //TODO exceli burada oluştur
                Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlWorkBook = xlApp.Workbooks.Add(misValue);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                xlWorkSheet.Cells[1, 1] = "Application Name";
                xlWorkSheet.Cells[1, 2] = "Email";
                xlWorkSheet.Cells[1, 3] = "Username";
                xlWorkSheet.Cells[2, 1] = userState.Application;
                xlWorkSheet.Cells[2, 3] = userState.UserName;
                xlWorkSheet.Cells[2, 2] = userState.Email;

                xlApp.DisplayAlerts = false;
                xlWorkBook.SaveAs("C:\\Users\\GunsuEskicioglu\\Desktop\\DaimlerChatbot\\" + userState.UserName + ".xlsx", Excel.XlFileFormat.xlOpenXMLWorkbook, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                xlWorkBook.Close(true, misValue, misValue);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlWorkSheet);
                Marshal.ReleaseComObject(xlWorkBook);
                Marshal.ReleaseComObject(xlApp);
            }
            else if (approveText == "hayır")
            {
                await turnContext.SendActivityAsync($"{userState.UserName} talebiniz iptal edilmiştir.");
                conversationState.CurrentState = PdwResetStates.Completed;

            }
            else
            {
                await turnContext.SendActivityAsync($"Sizi anlayamadım");
            }
        }

        private async Task<UserPasswordInfo> GetInfoOperations(ITurnContext<IMessageActivity> turnContext, PwdResetConversationStates conversationState, UserPasswordInfo userState)
        {
            UserPasswordInfo userStateFromAdaptiveCard = JsonConvert.DeserializeObject<UserPasswordInfo>(turnContext.Activity.Value.ToString());

            // Kullanıcı adı validasyonu
            if (!UserNameValidator.Validate(userStateFromAdaptiveCard.UserName))
            {
                await turnContext.SendActivityAsync("Kullanıcı adınızı hatalı girdiniz. Lütfen bilgileri tekrar giriniz");
                userStateFromAdaptiveCard.Counter++;
                if (userStateFromAdaptiveCard.Counter < 3)
                {
                    Activity t = new Activity();
                    t.Type = ActivityTypes.Message;
                    t.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson() };
                    await turnContext.SendActivityAsync(t);
                }
                else
                {
                    await turnContext.SendActivityAsync("3 kez hatalı giriş yaptınız.");
                    conversationState.CurrentState = PdwResetStates.Completed;
                }   

            }
            else if (!EmailValidator.Validate(userStateFromAdaptiveCard.Email))
            {
                await turnContext.SendActivityAsync("Email bilgisini hatalı girdiniz. Lütfen bilgileri tekrar giriniz");
                userStateFromAdaptiveCard.Counter++;
                if (userStateFromAdaptiveCard.Counter < 3)
                {
                    Activity t = new Activity();
                    t.Type = ActivityTypes.Message;
                    t.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson() };
                    await turnContext.SendActivityAsync(t);
                }
                else
                {
                    await turnContext.SendActivityAsync("3 kez hatalı giriş yaptınız.");
                    conversationState.CurrentState = PdwResetStates.Completed;
                }

            }
            else if(!ApplicationValidator.Validate(userStateFromAdaptiveCard.Application))
            {
                await turnContext.SendActivityAsync("Application bilgisini hatalı girdiniz. Lütfen bilgileri tekrar giriniz");
                userStateFromAdaptiveCard.Counter++;
                if (userStateFromAdaptiveCard.Counter < 3)
                {
                    Activity t = new Activity();
                    t.Type = ActivityTypes.Message;
                    t.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson() };
                    await turnContext.SendActivityAsync(t);
                }
                else
                {
                    await turnContext.SendActivityAsync("3 kez hatalı giriş yaptınız.");
                    conversationState.CurrentState = PdwResetStates.Completed;
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{userStateFromAdaptiveCard.UserName} için şifreniz resetlenecektir. Onaylıyor musunuz?");

                conversationState.CurrentState = PdwResetStates.GetApproval;

            }
            return userStateFromAdaptiveCard;

        }

        public async override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _convStateAccesor.SaveChangesAsync(turnContext, true, cancellationToken);
            await _userStateAccesor.SaveChangesAsync(turnContext, true, cancellationToken);
        }

        private async Task InitialStateOperations(ITurnContext<IMessageActivity> turnContext, PwdResetConversationStates conversationState, UserPasswordInfo userState, CancellationToken cancellationToken)
        {
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


            }
            else
            {
                await turnContext.SendActivityAsync("Talebinizi gerçekleştirebilmemiz için aşağıdaki bilgileri doldurunuz.");

                Activity t = new Activity();
                t.Type = ActivityTypes.Message;
                t.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson() };
                await turnContext.SendActivityAsync(t, cancellationToken);

                conversationState.CurrentState = PdwResetStates.GetInfo;
            }
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
