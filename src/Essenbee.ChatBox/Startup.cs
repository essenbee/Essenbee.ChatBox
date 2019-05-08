// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using Essenbee.ChatBox.Clients.GraphQL;
using Essenbee.ChatBox.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Essenbee.ChatBox
{
    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private readonly bool _isProduction;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IStorage dataStore = new MemoryStorage(); // For dev/test only

            // Create and add conversation state.
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);

            services.AddSingleton(conversationState);
            services.AddSingleton(userState);
            services.AddScoped<IChannelClient, ChannelGraphClient>();

            var appId = Configuration.GetSection("MicrosoftAppId").Value;
            var appPassword = Configuration.GetSection("MicrosoftAppPassword").Value;

            services.AddBot<ChatBoxBot>(options =>
           {
               options.CredentialProvider = new SimpleCredentialProvider(appId, appPassword);

                // Catches any errors that occur during a conversation turn and logs them to currently
                // configured ILogger.
                ILogger logger = _loggerFactory.CreateLogger<ChatBoxBot>();

               options.OnTurnError = async (context, exception) =>
               {
                   logger.LogError($"Exception caught : {exception}");
                   await context.SendActivityAsync("Sorry, it looks like something went wrong.");
               };
           });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseBotFramework();
        }
    }
}
