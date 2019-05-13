using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Dialogs
{
    public class CancelAndHelpDialog : ComponentDialog
    {
        public CancelAndHelpDialog(string dialogId) : base(dialogId)
        {

        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDialogContext, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDialogContext, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDialogContext, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDialogContext, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDialogContext, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDialogContext, CancellationToken cancellationToken)
        {
            if (innerDialogContext.Context.Activity.Type == ActivityTypes.Message && 
                innerDialogContext.Context.Activity?.Text != null)
            {
                var text = innerDialogContext.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "help":
                    case "?":
                        if (innerDialogContext.Stack != null && innerDialogContext.Stack.Count > 0)
                        {
                            var currentDialog = innerDialogContext.Stack[innerDialogContext.Stack.Count - 1]?.Id;

                            if (!string.IsNullOrWhiteSpace(currentDialog))
                            {
                                await innerDialogContext.Context.SendActivityAsync($"Show Help for {currentDialog}", cancellationToken: cancellationToken);
                                innerDialogContext.Context.Activity.Text = null;

                                return await innerDialogContext.ReplaceDialogAsync(currentDialog, cancellationToken);
                            }
                        }

                        break;
                    case "cancel":
                    case "quit":
                    case "stop":
                    case "menu":
                        await innerDialogContext.Context.SendActivityAsync($"Cancelling...", cancellationToken: cancellationToken);
                        return await innerDialogContext.CancelAllDialogsAsync();
                }
            }

            return null;
        }
    }
}
