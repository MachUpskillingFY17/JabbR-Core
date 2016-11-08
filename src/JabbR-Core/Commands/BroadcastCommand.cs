using System;
using JabbR_Core.Data.Models;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Commands
{
    [Command("broadcast", "Broadcast_CommandInfo", "message", "global")]
    public class BroadcastCommand : AdminCommand
    {
        public override void ExecuteAdminOperation(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            string messageText = String.Join(" ", args).Trim();

            if (String.IsNullOrEmpty(messageText))
            {
                throw new HubException(LanguageResources.Broadcast_MessageRequired);
            }

            context.NotificationService.BroadcastMessage(callingUser, messageText);
        }
    }
}