using Essenbee.ChatBox.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Core.Interfaces
{
    public interface IChannelClient
    {
        Task<ChannelModel> GetChannelByName(string channelName, string userTimeZone);
        Task<List<ChannelModel>> GetLiveChannels(string userTimeZone);
    }
}
