using System;
using System.Linq;
using JabbR_Core.Data.Models;

namespace JabbR_Core.Services
{
    public static class CacheExtensions
    {
        public static T Get<T>(this ICache cache, string key)
        {
            return (T)cache.Get(key);
        }

        public static bool? IsUserInRoom(this ICache cache, ChatUser user, ChatRoom room)
        {
            string key = CacheKeys.GetUserInRoom(user, room);

            // Cache is not implemetned.
            return (bool?)cache.Get(key);
        }

        public static void SetUserInRoom(this ICache cache, ChatUser user, ChatRoom room, bool value)
        {
            string key = CacheKeys.GetUserInRoom(user, room);

            // cache very briefly.  We could set this much higher if we know that we're on a non-scaled-out server.
            cache.Set(key, value, TimeSpan.FromSeconds(1));
        }

        public static void RemoveUserInRoom(this ICache cache, ChatUser user, ChatRoom room)
        {
            cache.Remove(CacheKeys.GetUserInRoom(user, room));
        }

        private static class CacheKeys
        {
            public static string GetUserInRoom(ChatUser user, ChatRoom room)
            {
                return "UserInRoom" + user.Key + "_" + room.Key;
            }
        }
    }
}