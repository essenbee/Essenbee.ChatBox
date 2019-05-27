using AdaptiveCards;
using Essenbee.ChatBox.Core.Interfaces;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Essenbee.ChatBox.Cards
{
    public static class TagSelectCard
    {
        public static Attachment Create(IChannelClient client)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var container = new AdaptiveContainer();
            container.Items.Add(new AdaptiveTextBlock
            {
                Text = "Which tags do you wish to search for?",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Medium,
            });

            var choices = new List<AdaptiveChoice>();
            var tagsInUse = client.GetTagsInUse().Result;

            foreach (var tag in tagsInUse)
            {
                var choice = new AdaptiveChoice
                {
                    Title = tag.Name,
                    Value = tag.Id.ToString(),
                };

                choices.Add(choice);
            }

            container.Items.Add(new AdaptiveChoiceSetInput
            {
                Id = "tags",
                Style = AdaptiveChoiceInputStyle.Expanded,
                Choices = choices,
                IsMultiSelect = true,
                Value = "US",
            });

            adaptiveCard.Body.Add(container);
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction
            {
                Id = "tags-submit",
                Title = "Submit",
                Type = "Action.Submit",
            });

            var attachment = new Attachment
            {
                Content = adaptiveCard,
                ContentType = "application/vnd.microsoft.card.adaptive",
            };

            return attachment;
        }
    }
}
