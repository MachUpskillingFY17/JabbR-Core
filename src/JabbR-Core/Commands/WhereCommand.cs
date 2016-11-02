using JabbR_Core.Data.Models;
using JabbR_Core.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Commands
{
    [Command("where", "Where_CommandInfo", "nickname", "user")]
    public class WhereCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0)
            {
                throw new HubException(LanguageResources.Where_UserRequired);
            }

            string targetUserName = args[0];

            ChatUser user = context.Repository.VerifyUser(targetUserName);
            context.NotificationService.ListRooms(user);
        }
    }
}
