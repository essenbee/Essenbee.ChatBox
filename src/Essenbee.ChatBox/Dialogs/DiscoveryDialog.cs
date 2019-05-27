using Essenbee.ChatBox.Cards;
using Essenbee.ChatBox.Core.Interfaces;
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
    public class DiscoveryDialog : CancelAndHelpDialog
    {
        private readonly IChannelClient _client;

        public DiscoveryDialog(string dialogId, IChannelClient client) : base(dialogId)
        {
            _client = client;

            var steps = new WaterfallStep[]
            {
                GetUsersInterestsStepAsync,
                GetStreamersStepAsync,
            };

            AddDialog(new WaterfallDialog(Constants.DiscoverIntent, steps));
            AddDialog(new TextPrompt(Constants.TagsPrompt));
        }

        private async Task<DialogTurnResult> GetUsersInterestsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Please select your interests and hit the 'Submit' button"),
                cancellationToken);

            var cardAttachment = TagSelectCard.Create(_client);
            var reply = stepContext.Context.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { cardAttachment };
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.PromptAsync(Constants.TagsPrompt,
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

        private async Task<DialogTurnResult> GetStreamersStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null && stepContext.Result is string)
            {
                var result = (string)stepContext.Result;
                if (result.TryParseJson(out JObject tagsJson))
                {
                    if (tagsJson.ContainsKey("tags"))
                    {
                        var typing = stepContext.Context.Activity.CreateReply();
                        typing.Type = ActivityTypes.Typing;
                        typing.Text = null;
                        
                        var tagsAsString = tagsJson["tags"].ToString();

                        try
                        {
                            var channels = await _client.GetChannelsHavingTags(tagsAsString);

                            if (channels.Any())
                            {
                                var reply = stepContext.Context.Activity.CreateReply();
                                reply.Attachments = new List<Attachment>
                                {
                                    ChannelListCard.Create(channels, "Channels of Interest"),
                                };
                                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync(
                                    MessageFactory.Text("Sorry, I can't find any live coding streamers that match your interests."));
                            }
                        }
                        catch (Exception)
                        {
                            await stepContext.Context.SendActivityAsync(
                                MessageFactory.Text($"I'm sorry, but I am having problems talking to the Dev Streams database."));
                        }
                    }

                    return await stepContext.EndDialogAsync();
                }
            }

            return await stepContext.ReplaceDialogAsync(Constants.DiscoverIntent, cancellationToken);
        }
    }
}
