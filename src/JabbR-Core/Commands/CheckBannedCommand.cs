using System;
using JabbR_Core.Services;
using JabbR_Core.Data.Models;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Commands
{
    [Command("checkbanned", "CheckBanned_CommandInfo", "[nickname]", "admin")]
    public class CheckBannedCommand : AdminCommand
    {
        public override void ExecuteAdminOperation(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0)
            {
                context.NotificationService.CheckBanned();
                return;
            }

            var name = MembershipService.NormalizeUserName(args[0]);

            ChatUser user = context.Repository.GetUserByName(name);

            if (user == null)
            {
                throw new HubException(String.Format(LanguageResources.UserNotFound, name));
            }

            context.NotificationService.CheckBanned(user);
        }
    }
}