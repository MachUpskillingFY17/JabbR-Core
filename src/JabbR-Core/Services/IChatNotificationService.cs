using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Models;
namespace JabbR_Core.Services
{
    public interface IChatNotificationService
    {
        void OnUserNameChanged(ChatUser user, string oldUserName, string newUserName);
        void UpdateUnreadMentions(ChatUser mentionedUser, int unread);
    }
}
