using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace QnAMakerDialog.Sample.Dialogs
{
    [Serializable]
    [QnAMakerService("975b91a189dd4a878543d802c976e94a", "ff127d71-818b-4c6a-8c6a-debd0633fab0")]
    public class QnADialog : QnAMakerDialog<object>
    {
        /// <summary>
        /// Handler used when the QnAMaker finds no appropriate answer
        /// </summary>
        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.PostAsync($"Ich konnte leider keine Antwort auf Ihre Frage finden: '{originalQueryText}'.");
            context.Wait(MessageReceived);
        }

        /// <summary>
        /// This is the default handler used if no specific applicable score handlers are found
        /// </summary>
        public override async Task DefaultMatchHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            // ProcessResultAndCreateMessageActivity will remove any attachment markup from the results answer
            // and add any attachments to a new message activity with the message activity text set by default
            // to the answer property from the result
            var messageActivity = ProcessResultAndCreateMessageActivity(context, ref result);
            messageActivity.Text = result.Answer;

            await context.PostAsync(messageActivity);

            //if event present
            PromptDialog.Confirm(context, Confirmed, "Ich hab eine passendes Event zu dem Thema gefunden: Event XY, am 7.7.2017. Möchten Sie teilnehmen?");

 

        }
        public async Task Confirmed(IDialogContext context, IAwaitable<bool> argument)
        {
            bool isYes = await argument;
            if (isYes)
            {
                context.Call(new RegisterEventDialog(), this.AfterEventRegister);
            }
            else
            {
                context.Wait(MessageReceived);
            }
        }

        private async Task AfterEventRegister(IDialogContext context, IAwaitable<object> result)
        {
            var sucess = await result;
            if ((bool)sucess)
            {
                await context.PostAsync("Sie sind angemeldet!");
            }
            else {
                await context.PostAsync("Anmeldung abgebrochen");

            }
            await context.PostAsync("Haben Sie weitere Fragen?");

            context.Wait(MessageReceived);

        }

        /// <summary>
        /// Handler to respond when QnAMakerResult score is a maximum of 50
        /// </summary>
        [QnAMakerResponseHandler(50)]
        public async Task LowScoreHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            var messageActivity = ProcessResultAndCreateMessageActivity(context, ref result);
            messageActivity.Text = $"Diese Antwort könnte hilfreich sein... {result.Answer}.";
            await context.PostAsync(messageActivity);

            context.Wait(MessageReceived);
        }
    }
}