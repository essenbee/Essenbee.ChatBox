using AdaptiveCards;
using Microsoft.Bot.Schema;
using TimeZoneNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Cards
{
    public static class CountryCard
    {
        public static Attachment Create()
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1,0));
            var container = new AdaptiveContainer();
            container.Items.Add(new AdaptiveTextBlock
            {
                Text = "Please select your country",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Large,
            });

            var choices = new List<AdaptiveChoice>();
            var countries = TZNames.GetCountryNames("en-us");

            foreach (var country in countries)
            {
                var choice = new AdaptiveChoice
                {
                    Title = country.Value,
                    Value = country.Key,
                };

                choices.Add(choice);
            }

            container.Items.Add(new AdaptiveChoiceSetInput
            {
                Id = "country",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices = choices,
            });

            adaptiveCard.Body.Add(container);
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction
            {
                Id = "country-submit",
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
