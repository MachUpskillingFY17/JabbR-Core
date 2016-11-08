using System;
using JabbR_Core.Data.Models;
using Microsoft.AspNetCore.SignalR.Hubs;
using JabbR_Core.Services;

namespace JabbR_Core.Commands
{
    [Command("flag", "Flag_CommandInfo", "Iso 3166-2 Code", "user")]
    public class FlagCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0)
            {
                // Clear the flag.
                callingUser.Flag = null;
            }
            else
            {
                // Set the flag.
                string isoCode = String.Join(" ", args[0]).ToLowerInvariant();
                ChatService.ValidateIsoCode(isoCode);
                callingUser.Flag = isoCode;
            }

            context.NotificationService.ChangeFlag(callingUser);
            
            context.Repository.CommitChanges();
        }
    }
}