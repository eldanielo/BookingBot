using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Linq;

namespace QnAMakerDialog.Sample.Dialogs
{
    [Serializable]
    public class RegisterEventDialog : IDialog<object>
    {
        private int attempts = 3;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Wenn Sie bereits Kunde sind geben Sie bitte ihre Kundenummer an. Ansonsten geben Sie uns bitte Ihren Namen bekannt");

            context.Wait(this.MessageReceivedAsync);
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.All(Char.IsLetter))
            {
                //assume name entered
                await context.PostAsync("Bitte geben Sie ihre Email Adresse an");
                context.Wait(this.EmailMessageReceivedAsync);
            }
            else {
                context.Done(true);
            }


          
        }

        private async Task EmailMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            --attempts;
            var message = await result;
            if (!message.Text.Contains("@") && attempts > 0)
            {
                await context.PostAsync("Bitte eine valide Email Adresse angeben!");
                context.Wait(this.EmailMessageReceivedAsync);

            }
            else if (attempts>0){ 
            context.Done(true);
            }
            else
            {
                context.Done(false);
            }
        }
    }
}