using AdaptiveCards;
using Essenbee.ChatBox.Core.Models;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

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

            if (channel.Schedule != null)
            {
                container.Items.Add(new AdaptiveTextBlock
                {
                    Text = "Schedule",
                    Weight = AdaptiveTextWeight.Bolder,
                });

                AdaptiveColumnSet schedule = FormatScheduleData(channel);
                container.Items.Add(schedule);
            }

            var nextStream = "currently has no next stream recorded in the Dev Streams calendar";

            if (channel.NextStream != null)
            {
                nextStream = FormatNextStream(channel);
            }

            container.Items.Add(new AdaptiveTextBlock
            {
                Text = $"**{channel.Name}** {nextStream}",
                Wrap = true,
            });

            adaptiveCard.Body.Add(container);

            var attachment = new Attachment
            {
                Content = adaptiveCard,
                ContentType = "application/vnd.microsoft.card.adaptive",
            };

            return attachment;
        }

        private static string FormatNextStream(ChannelModel channel)
        {
            if (channel.NextStream.UtcStartTime.Date == DateTime.UtcNow.Date)
            {
                return $"will be streaming next **today** at {channel.NextStream.LocalStartTime:h:mm tt}";
            }
            else if (channel.NextStream.UtcStartTime.Date == DateTime.UtcNow.Date.AddDays(1))
            {
                return $"will be streaming next **tomorrow** at {channel.NextStream.LocalStartTime:h:mm tt}";
            }

            return string.Format("will be streaming next on {0:dddd, MMMM dd} at {0:h:mm tt}",
                channel.NextStream.LocalStartTime, channel.NextStream.LocalStartTime);
        }

        private static AdaptiveColumnSet FormatScheduleData(ChannelModel channel)
        {
            var days = new AdaptiveColumn();
            days.Items.Add(new AdaptiveTextBlock
            {
                Text = $"Day of Week",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
            });

            days.Items.AddRange(channel.Schedule.Select(day => new AdaptiveTextBlock
            {
                Text = $"{day.DayOfWeek}",
            }));

            var from = new AdaptiveColumn();
            from.Items.Add(new AdaptiveTextBlock
            {
                Text = $"From",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
            });

            from.Items.AddRange(channel.Schedule.Select(day => new AdaptiveTextBlock
            {
                Text = $"{day.LocalStartTime}",
            }));

            var to = new AdaptiveColumn();
            to.Items.Add(new AdaptiveTextBlock
            {
                Text = $"To",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
            });

            to.Items.AddRange(channel.Schedule.Select(day => new AdaptiveTextBlock
            {
                Text = $"{day.LocalEndTime}",
            }));

            var schedule = new AdaptiveColumnSet();
            schedule.Columns.AddRange(new List<AdaptiveColumn> { days, from, to });
            return schedule;
        }
    }
}
