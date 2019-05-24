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

            AddDialog(new WaterfallDialog(Constants.SetTimezoneIntent, setTimezoneSteps));
            AddDialog(new ChooseCountryDialog(Constants.ChooseCountryIntent, userSelectionsState));
            AddDialog(new ChoseTimezoneDialog(Constants.ChooseTimeZoneIntent, userSelectionsState));
        }

        private async Task<DialogTurnResult> GetUsersCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(Constants.ChooseCountryIntent, cancellationToken);
        }

        private async Task<DialogTurnResult> GetUsersTimezoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if (!string.IsNullOrWhiteSpace(userSelections.CountryCode))
            {
                return await stepContext.BeginDialogAsync(Constants.ChooseTimeZoneIntent, cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(Constants.SetTimezoneIntent, cancellationToken);
        }
    }
}
