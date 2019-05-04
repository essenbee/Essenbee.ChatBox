# Essenbee.ChatBox

The aim of this project is to build an example conversational chat bot that that uses features such dialogs, rich cards and adaptive cards, to help users find information about live coding streamers on platforms such as Twitch. The bot is being developed live on my stream [Codebase Alpha](https://twitch.tv/codebasealpha) as part of a series of shows invesigating the [Microsoft Bot Framework][1]. The project started with the EchoBot template, and is gradually adding features, including any code migrations required or recommended for the latest version of the Bot Framework. Currently, we are using the v4.4.2 Nuget packages.

I am learning the Framework alongside my viewers, and so at any point in time, the code will be working, but will be in various stages of development or migration to the latest structural recommendations. We will add advanced features, such as AI, natural language recognition, and so fiorth, as their need is identified during the life of the project.

Follow me on Twitter `@codebasealpha` and on Twitch (at the above URL) to be notified when my next stream will be broadcast.

# Prerequisites
- [Visual Studio 2017 15.7][2] or newer installed.
- [.Net Core 2.1][3] or higher installed.  
- [Bot Framework Emulator 4.1][6] or newer installed

## Azure Deployment Prerequisites
This bot has prerequisite requirements in order to deploy the bot to Azure.

This document will enumerate the required prerequisites and show how to install them.

### Overview
There are a small set of CLI tools that will automate the process of deploying this bot to Azure.  These CLI tools are only require for deployment.  If you only plan to run the bot locally, these prerequisites are not required.

### Prerequisites
- If you don't have an Azure subscription, create a [free account][10].
- Install the latest version of the [Azure CLI][11] tool. Version 2.0.52 or higher.
- Install the latest `botservice` extension for the Azure CLI tool. Version 0.4.3 or higher.
    1. Ensure any previous version of the `botservice` extension is removed.
        ```bash
        az extension remove -n botservice
        ```
    1. Install the latest `botservice` extension.
        ```bash
        az extension add -n botservice
        ```
    1. Verify you are running version 0.4.3 or higher of the `botservice` extension.
        ```bash
        az extension list
        ```
        To verify the extension version, you should see the following:
        ```json
        {
            "extensionType": "whl",
            "name": "botservice",
            "version": "0.4.3"
        }
        ```
- Install latest version of the `MSBot` CLI tool. Version 4.2.0 or higher.

# Running Locally

## Visual Studio
- Open Essenbee.ChatBox.csproj in Visual Studio.
- Run the project (press `F5` key).

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator][5] is a desktop application that allows bot 
developers to test and debug their bots on localhost or running remotely through a tunnel.
- Install the [Bot Framework emulator][6].

## Connect to bot using Bot Framework Emulator **V4**
- Launch the Bot Framework Emulator.
- Tap the `Open Bot` button.
- In the dialog box, rovide the URL where the bot is running locally, adding `/api/messages` to the URL.
- Tap `Connect`.

# Deploy the bot to Azure
See [Deploy your C# bot to Azure][50] for instructions.

The deployment process assumes you have an account on Microsoft Azure and are able to log into the [Microsoft Azure Portal][60].

If you are new to Microsoft Azure, please refer to [Getting started with Azure][70] for guidance on how to get started on Azure.

# Further reading
* [Bot Framework Documentation][80]
* [Bot Basics][90]
* [Azure Bot Service Introduction][100]
* [Azure Bot Service Documentation][110]
* [Azure CLI][120]
* [msbot CLI][130]
* [Azure Portal][140]
* [Language Understanding using LUIS][150]

[1]: https://dev.botframework.com
[2]: https://docs.microsoft.com/en-us/visualstudio/releasenotes/vs2017-relnotes
[3]: https://dotnet.microsoft.com/download/dotnet-core/2.1
[5]: https://github.com/microsoft/botframework-emulator
[6]: https://aka.ms/botframeworkemulator

[10]: https://azure.microsoft.com/free/
[11]: https://docs.microsoft.com/cli/azure/install-azure-cli?view=azure-cli-latest

[50]: https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0
[60]: https://portal.azure.com
[70]: https://azure.microsoft.com/get-started/
[80]: https://docs.botframework.com
[90]: https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0
[100]: https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0
[110]: https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0
[120]: https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest
[130]: https://github.com/Microsoft/botbuilder-tools/tree/master/packages/MSBot
[140]: https://portal.azure.com
[150]: https://www.luis.ai
