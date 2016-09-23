﻿using System;
using System.Linq;
using JabbR_Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Commands
{
    [Command("open", "Open_CommandInfo", "room", "room")]
    public class OpenCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0)
            {
                throw new HubException(LanguageResources.Open_RoomRequired);
            }

            string roomName = args[0];
            ChatRoom room = context.Repository.VerifyRoom(roomName, mustBeOpen: false);

            context.Service.OpenRoom(callingUser, room);

            // join the room, unless already in the room
            if (!room.Users.Contains(callingUser))
            {
                context.Service.JoinRoom(callingUser, room, inviteCode: null);

                context.Repository.CommitChanges();

                context.NotificationService.JoinRoom(callingUser, room);
            }

            var users = room.Users.ToList();

            context.NotificationService.UnCloseRoom(users, room);
        }
    }
}