using AdaptiveCards;
using Essenbee.ChatBox.Core.Models;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Essenbee.ChatBox.Cards
{
    public static class ChannelListCard
    {
        public static Attachment Create(List<ChannelModel> channels, string title)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var container = new AdaptiveContainer();
            container.Items.Add(new AdaptiveTextBlock
            {
                Text = title,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.ExtraLarge,
                Separator = true,
            });

            foreach (var channel in channels)
            {
                container.Items.Add(new AdaptiveTextBlock
                {
                    Text = $"{channel.Name}",
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                    Size = AdaptiveTextSize.Large,
                });

                container.Items.Add(new AdaptiveTextBlock
                {
                    Text = $"{channel.Uri}",
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                });

                var tags = string.Join(", ", channel.Tags.Select(x => x.Name));

                container.Items.Add(new AdaptiveTextBlock
                {
                    Text = $"{tags}",
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                    Wrap = true,
                });
            }

            adaptiveCard.Body.Add(container);

            var attachment = new Attachment
            {
                Content = adaptiveCard,
                ContentType = "application/vnd.microsoft.card.adaptive",
            };

            return attachment;
        }
    }
}
