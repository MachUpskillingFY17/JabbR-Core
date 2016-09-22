using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

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
            return user.Rooms.Select(r => r.Name).ToList();
        }

        public static void EnsureAllowed(this ChatUser user, ChatRoom room)
        {
            if (room.Private && !room.IsUserAllowed(user))
            {
                throw new HubException(String.Format(LanguageResources.RoomAccessPermission, room.Name));
            }
        }

        public static bool IsUserAllowed(this ChatRoom room, ChatUser user)
        {
            // JC: Find the relationship between room and user
            var allowedUser = room.AllowedUsers.Where(r => (room.Key == r.ChatRoomKey) && (user.Key == r.ChatUserKey));
            var ownerUser = room.Owners.Where(r => (room.Key == r.ChatRoomKey) && (user.Key == r.ChatUserKey));

            // Ensure relationship is in list of allowedUsers
            // We can use .First() becasue the ChatRoomKey and ChatUserKey are primary keys and combined they will only return one unique value
            return room.AllowedUsers.Contains(allowedUser.First()) || room.Owners.Contains(ownerUser.First()) || user.IsAdmin;
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