using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Dialogs
{
    public class SetTimezoneDialog : CancelAndHelpDialog
    {
        public IStatePropertyAccessor<UserSelections> UserSelectionsState;

        public SetTimezoneDialog(string dialogId, IStatePropertyAccessor<UserSelections> userSelectionsState) : base(dialogId)
        {
            UserSelectionsState = userSelectionsState;

            var setTimezoneSteps = new WaterfallStep[]
            {
                GetUsersCountryStepAsync,
                GetUsersTimezoneStepAsync,
            };

            AddDialog(new WaterfallDialog("setTimezoneIntent", setTimezoneSteps));
            AddDialog(new ChooseCountryDialog("chooseCountryIntent", userSelectionsState));
            AddDialog(new ChoseTimezoneDialog("chooseTimezoneIntent", userSelectionsState));
        }

        private async Task<DialogTurnResult> GetUsersCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync("chooseCountryIntent", cancellationToken);
        }

        private async Task<DialogTurnResult> GetUsersTimezoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if (!string.IsNullOrWhiteSpace(userSelections.CountryCode))
            {
                return await stepContext.BeginDialogAsync("chooseTimezoneIntent", cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync("setTimezoneIntent", cancellationToken);
        }
    }
}
