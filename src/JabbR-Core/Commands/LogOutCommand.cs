using JabbR_Core.Data.Models;

namespace JabbR_Core.Commands
{
    [Command("logout", "Logout_CommandInfo", "", "user")]
    public class LogOutCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            //await _signInManager.SignOutAsync();

            //// redirect to AccountLogin since you are no longer authenticated
            //return this.Redirect("~/account/login");

            context.NotificationService.LogOut(callingUser, callerContext.ClientId);
        }
    }
}
