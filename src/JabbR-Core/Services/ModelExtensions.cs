using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using JabbR_Core.Data.Repositories;

namespace JabbR_Core.Data.Models
{
    public static class ModelExtensions
    {
        public static IList<string> GetConnections(this ChatUser user)
        {
            return user.ConnectedClients.Select(c => c.Id).ToList();
        }

        public static IList<string> GetRoomNames(this ChatUser user)
        {
            return user.Rooms.Select(r => r.ChatRoomKeyNavigation.Name).ToList();
        }

        public static void EnsureAllowed(this ChatUser user, ChatRoom room, IJabbrRepository repository)
        {
            user.AllowedRooms = repository.GetAllowedRooms(user).ToList();
            user.OwnedRooms = repository.GetOwnedRooms(user).ToList();

            if (room.Private && !room.IsUserAllowed(user))
            {
                throw new HubException(String.Format(LanguageResources.RoomAccessPermission, room.Name));
            }
        }

        public static bool IsUserAllowed(this ChatRoom room, ChatUser user)
        {
            // JC: Find the relationship between room and user
            var allowedUser = user.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).ToList().Contains(room);//room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).ToList().Contains(user);
            var ownerUser = user.OwnedRooms.Select(r => r.ChatRoomKeyNavigation).ToList().Contains(room); //room.Owners.Select(r => r.ChatUserKeyNavigation).ToList().Contains(user);

            // Ensure relationship is in list of allowedUsers
            // We can use .First() becasue the ChatRoomKey and ChatUserKey are primary keys and combined they will only return one unique value
            return allowedUser || ownerUser || user.IsAdmin;
        }

        public static void EnsureOpen(this ChatRoom room)
        {
            if (room.Closed)
            {
                throw new HubException(String.Format(LanguageResources.RoomClosed, room.Name));
            }
        }
    }
}