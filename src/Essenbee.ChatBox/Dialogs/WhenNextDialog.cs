using Essenbee.ChatBox.Cards;
using Essenbee.ChatBox.Core.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Dialogs
{
    public class WhenNextDialog : CancelAndHelpDialog
    {
        public IStatePropertyAccessor<UserSelections> UserSelectionsState;
        private readonly IChannelClient _client;

        public WhenNextDialog(string dialogId, IStatePropertyAccessor<UserSelections> userSelectionsState,
            IChannelClient client) : base(dialogId)
        {
            UserSelectionsState = userSelectionsState;
            _client = client;

            var whenNextSteps = new WaterfallStep[]
            {
                GetUsersTimezoneStepAsync,
                GetStreamerNameStepAsync,
                GetStreamerInfoStepAsync,
            };

            AddDialog(new WaterfallDialog("whenNextIntent", whenNextSteps));
            AddDialog(new SetTimezoneDialog("setTimezoneIntent", UserSelectionsState));
            AddDialog(new TextPrompt("streamer-name"));
        }

        private async Task<DialogTurnResult> GetUsersTimezoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);

            if (!string.IsNullOrWhiteSpace(userSelections.TimeZone))
            {
                return await stepContext.NextAsync();
            }

            return await stepContext.BeginDialogAsync("setTimezoneIntent", cancellationToken);
        }

        private async Task<DialogTurnResult> GetStreamerNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var streamerName = stepContext.Options as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(streamerName))
            {
                return await stepContext.PromptAsync("streamer-name", new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please enter the name of the streamer you are interested in")
                    }, 
                    cancellationToken);
            }

            return await stepContext.NextAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> GetStreamerInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            var streamerName = stepContext.Options as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(streamerName))
            {
                if (stepContext.Result is string)
                {
                    userSelections.StreamerName = (string)stepContext.Result;
                }
            }
            else
            {
                userSelections.StreamerName = streamerName;
            }

            if (!string.IsNullOrWhiteSpace(userSelections.StreamerName))
            {
                var typing = stepContext.Context.Activity.CreateReply();
                typing.Type = ActivityTypes.Typing;
                typing.Text = null;

                await stepContext.Context.SendActivityAsync(typing);

                var streamName = userSelections.StreamerName.ToLower()
                    .Replace(" ", string.Empty);

                try
                {
                    var channel = await _client.GetChannelByName(streamName, userSelections.TimeZone);

                    if (channel != null)
                    {
                        var reply = stepContext.Context.Activity.CreateReply();
                        reply.Attachments = new List<Attachment> { ChannelDataCard.Create(channel) };
                        await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text($"I'm sorry, but I could not find {userSelections.StreamerName} in the Dev Streams database"));
                    }
                }
                catch (Exception)
                {
                    await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text($"I'm sorry, but I am having problems talking to the Dev Streams database."));
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text($"I'm sorry, but I could not find that streamer in the Dev Streams database"));
            }

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}
