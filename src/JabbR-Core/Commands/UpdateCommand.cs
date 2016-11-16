using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Data.Models;

namespace JabbR_Core.Commands
{
    [Command("update", "", "", "")]
    public class UpdateCommand : AdminCommand
    {
        public override void ExecuteAdminOperation(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            context.NotificationService.ForceUpdate();
        }
    }
}
