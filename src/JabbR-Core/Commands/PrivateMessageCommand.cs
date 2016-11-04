using JabbR_Core.Data.Models;
using JabbR_Core.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Commands
{
    [Command("msg", "Msg_CommandInfo", "@user message", "user")]
    public class PrivateMessageCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0 || String.IsNullOrWhiteSpace(args[0]))
            {
                throw new HubException(LanguageResources.Msg_UserRequired);
            }

            var toUserName = args[0];
            ChatUser toUser = context.Repository.VerifyUser(toUserName);

            if (toUser == callingUser)
            {
                throw new HubException(LanguageResources.Msg_CannotMsgSelf);
            }

            string messageText = String.Join(" ", args.Skip(1)).Trim();

            if (String.IsNullOrEmpty(messageText))
            {
                throw new HubException(String.Format(LanguageResources.Msg_MessageRequired, toUser.Name));
            }

            context.NotificationService.SendPrivateMessage(callingUser, toUser, messageText);
        }
    }
}
