using DemoBot.Models;
using DemoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        // Dialoglar her zaman bir balangıca ve sona sahip olmalıdır ve bir mantıksal yol içermelidir. InitialDialogId'yi belirleyerek başlarız ve dialoga nextAsync ile devam ederiz ve en son endDialogAsync ile bitiririz. Bunlardan biri eksik olursa dialog bir sonra nereye gideceğini bilmez ve hata alırız.
        #region Variables
        private readonly StateService _botStateService;

        #endregion

        public GreetingDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            _botStateService = stateService ?? throw new ArgumentNullException(nameof(stateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            
            var waterfallSteps = new WaterfallStep[]
               {
                   // Buradaki metotlarin sirasi cok onemli cunku waterfall promptlarinin hangi sirayla gosterilecegini belirliyor.
               InitialStepAsync,
               FinalStepAsync
               };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            // Set the startup Dialog
            // Hangi dialogun once baslayacagini InitialDialogId yi burada belirleyerek sagliyoruz.
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync (WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            if(string.IsNullOrEmpty(userProfile.Name))
            {
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is your name?")

                    }, cancellationToken);
                
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // Set the name
                userProfile.Name = (string)stepContext.Result;

                // Save any changes that might have occured during turn
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hi {0}. How can I help you today?", userProfile.Name)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
