using System.Linq;
using JabbR_Core.Data.Models;

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
    }
}
