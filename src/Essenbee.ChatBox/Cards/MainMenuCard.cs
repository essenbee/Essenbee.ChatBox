using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Essenbee.ChatBox.Cards
{
    public static class MainMenuCard
    {
        public static HeroCard Create(string selectedTimeZone)
        {
            return new HeroCard
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
        }
    }
}
