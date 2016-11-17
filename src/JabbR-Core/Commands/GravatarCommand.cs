using Microsoft.AspNetCore.SignalR;
using System;
using JabbR_Core.Data.Models;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Commands
{
    [Command("gravatar", "Gravatar_CommandInfo", "email", "user")]
    public class GravatarCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            string email = String.Join(" ", args);

            if (String.IsNullOrWhiteSpace(email))
            {
                throw new HubException(LanguageResources.Gravatar_EmailRequired);
            }

            string hash = email.ToLowerInvariant().ToMD5();

            // Set user hash
            callingUser.Hash = hash;

            context.NotificationService.ChangeGravatar(callingUser);

            context.Repository.CommitChanges();
        }
    }
}
