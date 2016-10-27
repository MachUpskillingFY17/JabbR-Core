using System;
using JabbR_Core.Data.Models;

namespace JabbR_Core.Services
{
    public interface IChatNotificationService
    {
        void OnUserNameChanged(ChatUser user, string oldUserName, string newUserName);
        void UpdateUnreadMentions(ChatUser mentionedUser, int unread);
    }
}