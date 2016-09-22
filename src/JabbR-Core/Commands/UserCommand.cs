using JabbR_Core.Hubs;
using JabbR_Core.Models;
using System;

namespace JabbR_Core.Commands
{
    /// <summary>
    /// Base class for commands that require a user
    /// </summary>
    public abstract class UserCommand : ICommand
    {
        void ICommand.Execute(CommandContext context, CallerContext callerContext, string[] args)
        {
            //_repository = new InMemoryRepository();

            ChatUser user = context.Repository.VerifyUserId(callerContext.UserId);
            //ChatUser user = new Models.ChatUser();
            //ChatUser user = new ChatUser
            //{
            //    Id = "1",
            //    Name = "user1",
            //    LastActivity = Convert.ToDateTime("2016-08-23 00:26:35.713"),
            //    IsAdmin = true,
            //    IsAfk = true
            //};
            //var Room = new ChatRoom { Name = "light_meow" };


            Execute(context, callerContext, user, args);
        }

        public abstract void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args);
    }
}