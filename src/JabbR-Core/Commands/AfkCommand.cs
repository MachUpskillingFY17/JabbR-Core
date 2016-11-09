using JabbR_Core.Services;
using JabbR_Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Commands
{
    [Command("afk", "AFK_CommandInfo", "[note]", "user")]
    public class AfkCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            string message = String.Join(" ", args).Trim();

            ChatService.ValidateNote(message);

            callingUser.AfkNote = String.IsNullOrWhiteSpace(message) ? null : message;
            callingUser.IsAfk = true;
            callingUser.Status = (int)UserStatus.Inactive;

            context.NotificationService.ChangeAfk(callingUser);

            context.Repository.CommitChanges();
        }
    }
}
