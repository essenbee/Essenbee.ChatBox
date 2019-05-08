using AdaptiveCards;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneNames;

namespace Essenbee.ChatBox.Cards
{
    public static class TimezoneCard
    {
        public static Attachment Create(string countryCode)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var container = new AdaptiveContainer();
            container.Items.Add(new AdaptiveTextBlock
            {
                Text = "Please select your time zone",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Large,
            });

            var choices = new List<AdaptiveChoice>();
            var timezones = TZNames.GetTimeZonesForCountry(countryCode, "en-us");
            var defaultChoice = new AdaptiveChoice
            {
                Title = "Universal Co-ordinated Time",
                Value = "Etc/GMT",
            };

            if (timezones.Any())
            {
                foreach (var timezone in timezones)
                {
                    var choice = new AdaptiveChoice
                    {
                        Title = timezone.Value,
                        Value = timezone.Key,
                    };

                    choices.Add(choice);
                }
            }
            else
            {
                choices.Add(defaultChoice);
            }

            container.Items.Add(new AdaptiveChoiceSetInput
            {
                Id = "tz",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices = choices,
                Value = choices.First().Value,
            });

            adaptiveCard.Body.Add(container);
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction
            {
                Id = "tz-submit",
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
