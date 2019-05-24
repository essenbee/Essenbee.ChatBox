using Essenbee.ChatBox.Core.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Dialogs
{
    public class LiveNowDialog : CancelAndHelpDialog
    {
        private readonly IChannelClient _client;

        public LiveNowDialog(string dialogId, IChannelClient client) : base(dialogId)
        {
            _client = client;

            var steps = new WaterfallStep[]
            {
                GetLiveStreamsStepAsync,
            };

            AddDialog(new WaterfallDialog(Constants.LiveNow, steps));
        }

        private async Task<DialogTurnResult> GetLiveStreamsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var typing = stepContext.Context.Activity.CreateReply();
            typing.Type = ActivityTypes.Typing;
            typing.Text = null;

            await stepContext.Context.SendActivityAsync(typing);

            try
            {
                var channels = await _client.GetLiveChannels();

                if (channels != null && channels.Count > 0)
                {
                    var reply = stepContext.Context.Activity.CreateReply();
                    //reply.Attachments = new List<Attachment> { ChannelDataCard.Create(channel) };

                    var listOfStreams = new StringBuilder();

                    foreach (var channel in channels)
                    {
                        listOfStreams.AppendLine($"{channel.Name} is live now ({channel.Uri})");
                    }

                    reply.Text = listOfStreams.ToString();
                    await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text($"There are no DevStreams live coding streams currently broadcasting."));
                }
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text($"I'm sorry, but I am having problems talking to the Dev Streams database."));
                await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text($"{ex.Message}"));
            }

            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}
