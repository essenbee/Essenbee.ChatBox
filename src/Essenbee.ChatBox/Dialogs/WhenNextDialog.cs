using Essenbee.ChatBox.Cards;
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
    public class WhenNextDialog : ComponentDialog
    {
        public IStatePropertyAccessor<UserSelections> UserSelectionsState;

        public WhenNextDialog(string dialogId, IStatePropertyAccessor<UserSelections> userSelectionsState) : base(dialogId)
        {
            UserSelectionsState = userSelectionsState;

            var whenNextSteps = new WaterfallStep[]
            {
                GetUsersCountryStepAsync,
                GetUsersTimezoneStepAsync,
                GetStreamerNameStepAsync,
                GetStreamerInfoStepAsync,
            };

            AddDialog(new WaterfallDialog("whenNextIntent", whenNextSteps));
            AddDialog(new TextPrompt("streamer-name"));
            AddDialog(new TextPrompt("country"));
            AddDialog(new TextPrompt("timezone"));
        }

        private async Task<DialogTurnResult> GetUsersCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if(!string.IsNullOrWhiteSpace(userSelections.CountryCode))
            {
                return await stepContext.NextAsync();
            }

            var cardAttachment = CountryCard.Create();
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { cardAttachment };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync("country",
                new PromptOptions
                { Prompt = new Activity
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

            if (!string.IsNullOrWhiteSpace(userSelections.TimeZone))
            {
                return await stepContext.NextAsync();
            }

            var countryJson = JObject.Parse((string)stepContext.Result);
            if (countryJson.ContainsKey("country"))
            {
                userSelections.CountryCode = countryJson["country"].ToString();
            }

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

        private async Task<DialogTurnResult> GetStreamerNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if (string.IsNullOrWhiteSpace(userSelections.TimeZone))
            {
                var timezoneJson = JObject.Parse((string)stepContext.Result);
                if (timezoneJson.ContainsKey("tz"))
                {
                    userSelections.TimeZone = timezoneJson["tz"].ToString();
                }

                await UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);
            }

            return await stepContext.PromptAsync("streamer-name", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the name of the streamer you are interested in")
            },
                cancellationToken);
        }

        private async Task<DialogTurnResult> GetStreamerInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            userSelections.StreamerName = (string)stepContext.Result;

            // ToDo: get the data from GraphQL endpoint

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You selected {userSelections.StreamerName}"),
                cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}
