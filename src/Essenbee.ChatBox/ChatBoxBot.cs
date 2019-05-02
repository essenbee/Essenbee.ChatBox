// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox
{
    public class ChatBoxBot : IBot
    {
        private string propertyName;
        private readonly ChatBoxAccessors _accessors;
        private readonly ILogger _logger;
        private DialogSet _dialogs;

        private const string WelcomeMessage = @"Welcome to the ChatBox. 
                                        This bot can help you find out about 
                                        live coding streams on Twitch!";


        public ChatBoxBot(ConversationState conversationState, ChatBoxAccessors accessors,
            ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<ChatBoxBot>();
            _logger.LogTrace("Turn start.");
            _accessors = accessors;

            _dialogs = new DialogSet(_accessors.ConversationDialogState);

            var dummySteps = new WaterfallStep[]
            {
                DummyStepAsync,
            };

            var whenNextSteps = new WaterfallStep[]
            {
                GetStreamerNameStepAsync,
                GetStreamerInfoStepAsync,
            };

            _dialogs.Add(new WaterfallDialog("dummy", dummySteps));
            _dialogs.Add(new WaterfallDialog("whenNextIntent", whenNextSteps));
            _dialogs.Add(new TextPrompt("streamer-name"));
        }

        private async Task<DialogTurnResult> GetStreamerInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await _accessors.UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            userSelections.StreamerName = (string)stepContext.Result;

            // ToDo: get the data from GraphQL endpoint

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You selected {userSelections.StreamerName}"), 
                cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> GetStreamerNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("streamer-name", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the name of the streamer you are interested in") }, 
                cancellationToken);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                var channelData = JObject.Parse(turnContext.Activity.ChannelData.ToString());

                var userChoice = turnContext.Activity.Text;
                var responseMessage = $"You chose: '{turnContext.Activity.Text}'\n";

                switch (results.Status)
                {
                    case DialogTurnStatus.Empty:

                        if (!string.IsNullOrWhiteSpace(userChoice))
                        {
                            switch (userChoice)
                            {
                                case "1":
                                    await dialogContext.BeginDialogAsync("dummy", cancellationToken);
                                    break;
                                case "2":
                                    await dialogContext.BeginDialogAsync("whenNextIntent", cancellationToken);
                                    break;
                                case "3":
                                    await dialogContext.BeginDialogAsync("dummy", cancellationToken);
                                    break;
                                case "4":
                                    await dialogContext.BeginDialogAsync("dummy", cancellationToken);
                                    break;
                                default:
                                    await turnContext.SendActivityAsync("Please select a menu option");
                                    await DisplayMainMenuAsync(turnContext, cancellationToken);
                                    break;
                            }
                        }

                        break;

                    case DialogTurnStatus.Cancelled:
                        await DisplayMainMenuAsync(turnContext, cancellationToken);
                        break;
                    case DialogTurnStatus.Waiting:
                        // Active dialog, so do nothing
                        break;
                    case DialogTurnStatus.Complete:
                        var userSelections = results.Result as UserSelections;
                        await _accessors.UserSelectionsState.SetAsync(turnContext, userSelections, cancellationToken);
                        await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
                        
                        await DisplayMainMenuAsync(turnContext, cancellationToken);
                        break;
                }







                // Get the conversation state from the turn context.
                var state = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count for this conversation.
                state.TurnCount++;

                // Set the property using the accessor.
                await _accessors.CounterState.SetAsync(turnContext, state);

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Not the bot!
                    await turnContext.SendActivityAsync($"Hi there! {WelcomeMessage}",
                        cancellationToken: cancellationToken);
                    await DisplayMainMenuAsync(turnContext, cancellationToken);
                }
            }
        }

        private static async Task DisplayMainMenuAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = turnContext.Activity.CreateReply("What would you like to do?");
            reply.SuggestedActions = new SuggestedActions
            {
                Actions = new List<CardAction>
                {
                    new CardAction { Title = "1. ", Type = ActionTypes.ImBack, Value = "1" },
                    new CardAction { Title = "2. ", Type = ActionTypes.ImBack, Value = "2" },
                    new CardAction { Title = "3. ", Type = ActionTypes.ImBack, Value = "3" },
                    new CardAction { Title = "4. Help", Type = ActionTypes.ImBack, Value = "4" },
                }
            };

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private async Task<DialogTurnResult> DummyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selection = stepContext.Context.Activity.Text;
            await stepContext.Context.
                SendActivityAsync(MessageFactory.Text($"You selected {selection}"), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
