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
using TimeZoneNames;

namespace Essenbee.ChatBox.Dialogs
{
    public class ChooseCountryDialog : CancelAndHelpDialog
    {
        public IStatePropertyAccessor<UserSelections> UserSelectionsState;

        public ChooseCountryDialog(string dialogId, IStatePropertyAccessor<UserSelections> userSelectionsState) : base(dialogId)
        {
            UserSelectionsState = userSelectionsState;

            var setTimezoneSteps = new WaterfallStep[]
            {
                GetUsersCountryStepAsync,
                PersistDataStepAsync,
            };

            AddDialog(new WaterfallDialog(Constants.ChooseCountryIntent, setTimezoneSteps));
            AddDialog(new TextPrompt(Constants.CountryPrompt));
        }

        private async Task<DialogTurnResult> GetUsersCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text != null)
            {
                stepContext.Context.Activity.Text = null;
                return await stepContext.NextAsync(cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Please select a valid country from the list and hit the 'Submit' button"),
                cancellationToken);

            var cardAttachment = CountryCard.Create();
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { cardAttachment };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync(Constants.CountryPrompt,
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
                userSelections.CountryCode = GetCountryCode(stepContext);

                await UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);

                return await stepContext.EndDialogAsync(cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(Constants.ChooseCountryIntent, cancellationToken);
        }

        private string GetCountryCode(WaterfallStepContext stepContext)
        {
            var result = (string)stepContext.Result;
            if (result.TryParseJson(out JObject countryJson))
            {
                if (countryJson.ContainsKey("country"))
                {
                    return countryJson["country"].ToString();
                }
            }
            else
            {
                var countries = TZNames.GetCountryNames("en-us");

                if (result.Length == 2)
                {
                    if (countries.Keys.Contains(result.ToUpper()))
                    {
                        return result.ToUpper();
                    }
                }
                else
                {
                    var code = countries.FirstOrDefault(c => c.Value.ToLower() == result.ToLower()).Key;

                    if (code != null)
                    {
                        return code;
                    }
                }
            }

            return string.Empty;
        }
    }
}
