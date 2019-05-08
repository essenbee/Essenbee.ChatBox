using AdaptiveCards;
using Essenbee.ChatBox.Core.Models;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;

namespace Essenbee.ChatBox.Cards
{
    public static class ChannelDataCard
    {
        public static Attachment Create(ChannelModel channel)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var container = new AdaptiveContainer();
            container.Items.Add(new AdaptiveTextBlock
            {
                Text = $"{channel.Name}",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.ExtraLarge,
            });

            container.Items.Add(new AdaptiveTextBlock
            {
                Text = $"{channel.Uri}",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Size = AdaptiveTextSize.Large,
            });

            container.Items.Add(new AdaptiveTextBlock
            {
                Text = "Schedule",
                Weight = AdaptiveTextWeight.Bolder,
            });

            var days = new AdaptiveColumn();
            days.Items.Add(new AdaptiveTextBlock
            {
                Text = $"Day of Week",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
            });

            foreach (var day in channel.Schedule)
            {
                days.Items.Add(new AdaptiveTextBlock
                {
                    Text = $"{day.DayOfWeek}",
                });
            }

            var from = new AdaptiveColumn();
            from.Items.Add(new AdaptiveTextBlock
            {
                Text = $"From",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
            });

            foreach (var day in channel.Schedule)
            {
                from.Items.Add(new AdaptiveTextBlock
                {
                    Text = $"{day.LocalStartTime}",
                });
            }

            var to = new AdaptiveColumn();
            to.Items.Add(new AdaptiveTextBlock
            {
                Text = $"To",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
            });

            foreach (var day in channel.Schedule)
            {
                to.Items.Add(new AdaptiveTextBlock
                {
                    Text = $"{day.LocalEndTime}",
                });
            }

            var schedule = new AdaptiveColumnSet();
            schedule.Columns.AddRange(new List<AdaptiveColumn> { days, from, to });
            container.Items.Add(schedule);

            var nextStream = "currently has no next stream recorded in the Dev Streams calendar";

            if (channel.NextStream.UtcStartTime.Date == DateTime.UtcNow.Date)
            {
                nextStream = string.Format("will be streaming next **today** at {0:h:mm tt}", channel.NextStream.LocalStartTime);
            }
            else if (channel.NextStream.UtcStartTime.Date == DateTime.UtcNow.Date.AddDays(1))
            {
                nextStream = string.Format("will be streaming next **tomorrow** at {0:h:mm tt}", channel.NextStream.LocalStartTime);
            }
            else
            {
                nextStream = string.Format("will be streaming next on {0:dddd, MMMM dd} at {0:h:mm tt}",
                channel.NextStream.LocalStartTime, channel.NextStream.LocalStartTime);
            }

            container.Items.Add(new AdaptiveTextBlock
            {
                Text = $"**{channel.Name}** {nextStream}",
            });

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
