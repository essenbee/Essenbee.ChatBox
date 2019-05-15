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
                PersistDataStepAsync,
                PersistDataStep2Async,
            };

            AddDialog(new WaterfallDialog("setTimezoneIntent", setTimezoneSteps));
            AddDialog(new TextPrompt("country"));
            AddDialog(new TextPrompt("timezone"));
        }

        private async Task<DialogTurnResult> GetUsersCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardAttachment = CountryCard.Create();
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { cardAttachment };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync("country",
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

        private async Task<DialogTurnResult> GetUsersTimezoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if (stepContext.Result != null && stepContext.Result is string)
            {
                userSelections.CountryCode = GetCountryCode(stepContext);

                await UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);

                var cardAttachment = TimezoneCard.Create(userSelections.CountryCode);
                var reply = stepContext.Context.Activity.CreateReply();
                reply.Attachments = new List<Attachment> { cardAttachment };
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return await stepContext.PromptAsync("timezone",
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
            else
            {
                return await stepContext.NextAsync();
            }
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

            // TODO: don't really want to do this, what I want, what I really, really want
            // is to replace this dialog step.
            return await stepContext.PromptAsync("timezone",
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

        // TODO: Remove this when replaying a dialog step is sorted
        private async Task<DialogTurnResult> PersistDataStep2Async(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Please select a time zone from the drop down list and hit 'Submit'"));
            return await stepContext.ContinueDialogAsync(cancellationToken);
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
