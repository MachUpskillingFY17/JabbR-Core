using JabbR_Core.Data.Models;
using JabbR_Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Commands
{
    [Command("topic", "Topic_CommandInfo", "[topic]", "room")]
    public class TopicCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            string newTopic = String.Join(" ", args).Trim();
            ChatService.ValidateTopic(newTopic);

            newTopic = String.IsNullOrWhiteSpace(newTopic) ? null : newTopic;

            ChatRoom room = context.Repository.VerifyUserRoom(context.Cache, callingUser, callerContext.RoomName);

            room.EnsureOpen();

            context.Service.ChangeTopic(callingUser, room, newTopic);

            context.NotificationService.ChangeTopic(callingUser, room);
        }
    }

}
