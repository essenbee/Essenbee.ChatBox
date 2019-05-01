// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;

namespace Essenbee.ChatBox
{
    public class ChatBoxAccessors
    {
        public ChatBoxAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public static string CounterStateName { get; } = $"{nameof(ChatBoxAccessors)}.CounterState";
        public static string DialogStateName { get; } = $"{nameof(ChatBoxAccessors)}.ConversationDialogState";
        public static string UserSelectionsStateName { get; } = $"{nameof(ChatBoxAccessors)}.UserSelections";

        public IStatePropertyAccessor<CounterState> CounterState { get; set; }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<UserSelections> UserSelectionsState { get; set; }

        public ConversationState ConversationState { get; }
        public UserState UserState { get; }
    }
}
