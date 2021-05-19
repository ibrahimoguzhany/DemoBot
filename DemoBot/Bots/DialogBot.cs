using DemoBot.Helpers;
using DemoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        #region Variables
        protected readonly Dialog _dialog;
        protected readonly StateService _stateService;
        protected readonly ILogger _logger;
        #endregion

        public DialogBot(StateService botstateService, T dialog, ILogger<DialogBot<T>> logger)
        {
            _stateService = botstateService ?? throw new ArgumentNullException(nameof(botstateService));


            // Save any state changes that might have occured during the turn.
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _logger = _logger ?? throw new ArgumentNullException(nameof(logger));

        }

        // Bir bot herhangi bir activity mesajin tipine bakmaksizin bu metot cagrilir. Ornegin bot bir mesaj aldiginda hem OnTurnAsync hem de OnMessageActivity cagrilir.
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            // buradaki base classdaki OnTurnAsync in cagirilmasi cok onemlidir cunku base classtaki OnTurnAsync ilgili mesajlarin ve tiplerin hooklarina iletilmesini saglar.

            await _stateService.UserState.SaveChangesAsync(turnContext, force: false, cancellationToken);
            await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");


            // Run the Dialog with the new message Activity.
            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }


    }
}
