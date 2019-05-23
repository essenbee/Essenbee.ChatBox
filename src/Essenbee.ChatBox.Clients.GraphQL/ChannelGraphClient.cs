using Essenbee.ChatBox.Core.Interfaces;
using Essenbee.ChatBox.Core.Models;
using GraphQL.Client;
using GraphQL.Common.Exceptions;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Essenbee.ChatBox.Clients.GraphQL
{
    public class ChannelGraphClient : IChannelClient
    {
        private readonly GraphQLClient _client;

        public ChannelGraphClient(IConfiguration config)
        {
            var endPoint = config["GraphQLEndpoint"];
            _client = new GraphQLClient(endPoint);
        }
        public async Task<ChannelModel> GetChannelByName(string channelName, string userTimeZone)
        {
            var query = new GraphQLRequest
            {
                Query = @"query getChannel($name: String!, $tz: String!) {
                            channelSoundex(name: $name) {
                            name uri countryCode
                            nextStream {
                                localStartTime(timeZone: $tz)
                                localEndTime(timeZone: $tz)
                                utcStartTime
                            }
                            schedule (timeZone: $tz) {
                                dayOfWeek localStartTime localEndTime
                            }
                            tags { 
                                id name
                            }
                        }
                }",
                Variables = new { name = channelName, tz = userTimeZone }
            };

            var response = await _client.PostAsync(query);

            if (response.Errors is null)
            {
                return response.GetDataFieldAs<ChannelModel>("channelSoundex");
            }

            var error = response.Errors.First();
            throw new GraphQLException(new GraphQLError { Message = $"Error: {error.Message}" });
         }

        public async Task<List<ChannelModel>> GetLiveChannels()
        {
            var query = new GraphQLRequest
            {
                Query = @"query getLiveChannels {
                            liveChannels {
                            name uri
                            tags { 
                                id name
                            }
                        }
                }"
            };

            var response = await _client.PostAsync(query);

            if (response.Errors is null)
            {
                return response.GetDataFieldAs<List<ChannelModel>>("liveChannels");
            }

            var error = response.Errors.First();
            throw new GraphQLException(new GraphQLError { Message = $"Error: {error.Message}" });
        }
    }
}
