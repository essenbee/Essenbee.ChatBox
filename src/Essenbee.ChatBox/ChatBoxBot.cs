using Essenbee.ChatBox.Core.Interfaces;
using Essenbee.ChatBox.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Essenbee.ChatBox
{
    public class ChatBoxBot : IBot
    {
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<UserSelections> UserSelectionsState { get; set; }

        private QnAMaker QnA { get; } = null;
        private LuisRecognizer Recognizer { get; } = null;

        private readonly ConversationState _converationState;
        private readonly UserState _userState;
        private readonly ILogger _logger;
        private DialogSet _dialogs;
        
        private const string WelcomeMessage = @"Welcome to the DevStreams ChatBox! 
                                        This bot can help you find out about live coding streams on Twitch.
                                        You can type 'help' at any time or 'menu' to return to the Main Menu.";

        public ChatBoxBot(ConversationState conversationState, UserState userState, IChannelClient client,
            QnAMaker qna, LuisRecognizer luis, ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _userState = userState;
            _converationState = conversationState;
            QnA = qna;
            Recognizer = luis;
            ConversationDialogState = _converationState.CreateProperty<DialogState>($"{nameof(ChatBox)}.ConversationDialogState");
            UserSelectionsState = _userState.CreateProperty<UserSelections>($"{nameof(ChatBox)}.UserSelectionsState");

            _logger = loggerFactory.CreateLogger<ChatBoxBot>();
            _logger.LogTrace("Turn start.");

            _dialogs = new DialogSet(ConversationDialogState);

            var dummySteps = new WaterfallStep[]
            {
                DummyStepAsync,
            };

            _dialogs.Add(new WhenNextDialog("whenNextIntent", UserSelectionsState, client));
            _dialogs.Add(new SetTimezoneDialog("setTimezoneIntent", UserSelectionsState));
            _dialogs.Add(new WaterfallDialog("dummy", dummySteps));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                var channelData = JObject.Parse(turnContext.Activity.ChannelData.ToString());

                if (channelData.ContainsKey("postBack"))
                {
                    // This is from an adaptive card postback
                    var activity = turnContext.Activity;
                    activity.Text = activity.Value.ToString();
                }

                var (intentMatched, options) = await ProcessLuisResult(turnContext, cancellationToken);
                var userChoice = string.IsNullOrWhiteSpace(intentMatched)
                    ? turnContext.Activity.Text
                    : intentMatched;

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
                                    await dialogContext.BeginDialogAsync("whenNextIntent", options, cancellationToken);
                                    break;
                                case "3":
                                    await dialogContext.BeginDialogAsync("dummy", cancellationToken);
                                    break;
                                case "4":
                                    await dialogContext.BeginDialogAsync("setTimezoneIntent", cancellationToken);
                                    break;
                                case "help":
                                    await turnContext.SendActivityAsync("<here's some help>");
                                    break;
                                default:
                                        var answers = await QnA.GetAnswersAsync(turnContext);

                                        if (answers.Any())
                                        {
                                            await turnContext.SendActivityAsync(answers[0].Answer);
                                        }

                                        await turnContext.SendActivityAsync("Please select a menu option");

                                    break;
                            }
                        }

                        break;

                    case DialogTurnStatus.Cancelled:
                        break;
                    case DialogTurnStatus.Waiting:
                        await dialogContext.ContinueDialogAsync(cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                        break;
                }

                await _converationState.SaveChangesAsync(turnContext);
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

                if (dialogContext.ActiveDialog is null)
                {
                    await DisplayMainMenuAsync(turnContext, cancellationToken);
                }
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

        private async Task<(string intent, string options)> ProcessLuisResult(ITurnContext turnContext, 
            CancellationToken cancellationToken)
        {
            var intentMatched = string.Empty;
            var options = string.Empty;

            var recognizerResult = await Recognizer.RecognizeAsync(turnContext, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();
            string strIntent = (topIntent != null) ? topIntent.Value.intent : string.Empty;
            double dblIntentScore = (topIntent != null) ? topIntent.Value.score : 0.0;

            if (dblIntentScore > 0.90)
            {
                switch (strIntent)
                {
                    case "Utilities_Help":
                        intentMatched = "help";
                        break;
                    case "WhenNext":
                        intentMatched = "2";

                        try
                        {
                            if (recognizerResult.Entities.HasValues)
                            {
                                options = recognizerResult.Entities
                                    ?.Last
                                    ?.Children()
                                    .FirstOrDefault()
                                    ?.Values<string>()
                                    .FirstOrDefault() ?? string.Empty;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogTrace($"Trying to determine streamer name: {ex.Message}");
                        }

                        break;
                    case "SetTimezone":
                        intentMatched = "4";
                        break;
                }
            }

            return (intentMatched, options);
        }

        private async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
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

        private async Task DisplayMainMenuAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userSelections = await UserSelectionsState.GetAsync(turnContext, () => new UserSelections(), cancellationToken);

            var selectedTimeZone = !string.IsNullOrWhiteSpace(userSelections.TimeZone) 
                ? $"**{userSelections.TimeZone}**"
                : "-";

            var heroCard = new HeroCard
            {
                Title = "DevStreams Chat Box",
                Subtitle = "What would you like to do?",
                Text = $"Your selected time zone: {selectedTimeZone}",
                Buttons = new List<CardAction>
                {
                    new CardAction { Title = "1. Find out who is live now", Type = ActionTypes.ImBack, Value = "1" },
                    new CardAction { Title = "2. Find out when a streamer is broadcasting next", Type = ActionTypes.ImBack, Value = "2" },
                    new CardAction { Title = "3. Discover live coding streams covering things I am interested in", Type = ActionTypes.ImBack, Value = "3" },
                    new CardAction { Title = "4. Set/reset my time zone", Type = ActionTypes.ImBack, Value = "4" },
                }
            };

            var reply = turnContext.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { heroCard.ToAttachment() };

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
