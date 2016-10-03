using System.Linq;
using JabbR_Core.Data.Models;
using System.Collections.Generic;

namespace JabbR_Core.Data.Repositories
{
    public static class RepositoryExtensions
    {
        public static IQueryable<ChatUser> Online(this IQueryable<ChatUserChatRooms> source)
        {
            var users = from s in source
                        where s.ChatUserKeyNavigation.Status != (int)UserStatus.Offline
                        select s.ChatUserKeyNavigation;

            return users;
        }

        public static IQueryable<ChatUser> Online(this IQueryable<ChatUser> source)
        {
            return source.Where(u => u.Status != (int)UserStatus.Offline);
        }

        public static IEnumerable<ChatUser> Online(this IEnumerable<ChatUserChatRooms> source)
        {
            var users = from s in source
                        where s.ChatUserKeyNavigation.Status != (int)UserStatus.Offline
                        select s.ChatUserKeyNavigation;

            return users;
        }

        public static IEnumerable<ChatUser> Online(this IEnumerable<ChatUser> source)
        {
            return source.Where(u => u.Status != (int)UserStatus.Offline);
        }
    }
}


        
