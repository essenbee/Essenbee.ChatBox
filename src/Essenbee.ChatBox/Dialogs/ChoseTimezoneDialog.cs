using Essenbee.ChatBox.Cards;
using Essenbee.ChatBox.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Dialogs
{
    public class ChoseTimezoneDialog : CancelAndHelpDialog
    {
        public IStatePropertyAccessor<UserSelections> UserSelectionsState;

        public ChoseTimezoneDialog(string dialogId, IStatePropertyAccessor<UserSelections> userSelectionsState) : base(dialogId)
        {
            UserSelectionsState = userSelectionsState;

            var setTimezoneSteps = new WaterfallStep[]
            {
                GetUsersTimezoneStepAsync,
                PersistDataStepAsync,
            };

            AddDialog(new WaterfallDialog(Constants.ChooseTimeZoneIntent, setTimezoneSteps));
            AddDialog(new TextPrompt(Constants.TimezonePrompt));
        }

        private async Task<DialogTurnResult> GetUsersTimezoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text != null)
            {
                stepContext.Context.Activity.Text = null;
                return await stepContext.NextAsync(cancellationToken);
            }

            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Please select a valid time zone from the list and hit the 'Submit' button"),
                cancellationToken);

            var cardAttachment = TimezoneCard.Create(userSelections.CountryCode);
                var reply = stepContext.Context.Activity.CreateReply();
                reply.Attachments = new List<Attachment> { cardAttachment };
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync(Constants.TimezonePrompt,
                    new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Text = string.Empty,
                            Type = ActivityTypes.Message,
                        }
                    },
                    cancellationToken);
        }

        private async Task<DialogTurnResult> PersistDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if (stepContext.Result != null && stepContext.Result is string)
            {
                var result = (string)stepContext.Result;
                if (result.TryParseJson(out JObject timezoneJson))
                {
                    if (timezoneJson.ContainsKey("tz"))
                    {
                        userSelections.TimeZone = timezoneJson["tz"].ToString();
                    }

                    await UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);
                    return await stepContext.EndDialogAsync();
                }
            }

            return await stepContext.ReplaceDialogAsync(Constants.ChooseTimeZoneIntent, cancellationToken);
        }
    }
}
