using Essenbee.ChatBox.Core.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Dialogs
{
    public class WhenNextDialog : ComponentDialog
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

        private async Task<DialogTurnResult> GetStreamerNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) => await stepContext.PromptAsync("streamer-name", new PromptOptions
        {
            Prompt = MessageFactory.Text("Please enter the name of the streamer you are interested in")
        },
                cancellationToken);

        private async Task<DialogTurnResult> GetStreamerInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            userSelections.StreamerName = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(ActivityTypes.Typing);

            var streamName = userSelections.StreamerName.ToLower()
                .Replace(" ", string.Empty);

            try
            {
                var channel = await _client.GetChannelByName(streamName, userSelections.TimeZone);

                if (channel != null)
                {
                    var schedule = new StringBuilder();
                    foreach (var day in channel.Schedule)
                    {
                        schedule.AppendLine($"{day.DayOfWeek}: {day.LocalStartTime} - {day.LocalEndTime}");
                    }

                    schedule.AppendLine();

                    var nextStream = "current not recorded in Dev Streams";

                    if (channel.NextStream.UtcStartTime.Date == DateTime.UtcNow.Date)
                    {
                        nextStream = string.Format("will be streaming next today at {0:h:mm tt}", channel.NextStream.LocalStartTime);
                    }
                    else if (channel.NextStream.UtcStartTime.Date == DateTime.UtcNow.Date.AddDays(1))
                    {
                        nextStream = string.Format("will be streaming next tomorrow at {0:h:mm tt}", channel.NextStream.LocalStartTime);
                    }
                    else
                    {
                        nextStream = string.Format("will be streaming next on {0:dddd, MMMM dd} at {0:h:mm tt}",
                        channel.NextStream.LocalStartTime, channel.NextStream.LocalStartTime);
                    }

                    schedule.AppendLine($"**Next stream**: {nextStream}");

                    var heroCard = new HeroCard
                    {
                        Title = $"{channel.Name}",
                        Subtitle = $"Link: {channel.Uri}",
                        Text = $"{schedule}",
                    };

                    var reply = stepContext.Context.Activity.CreateReply();
                    reply.Attachments = new List<Attachment> { heroCard.ToAttachment() };

                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text($"I'm sorry, but I could not find {userSelections.StreamerName} in the Dev Streams database"));
                }
            }
            catch (Exception ex)
            {
                var x = ex;
            }

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}
