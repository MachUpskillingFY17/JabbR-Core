using System;
using System.Linq;
using JabbR_Core.Models;
//using Attachment = JabbR_Core.Models.Attachment;
//using ChatClient = JabbR_Core.Models.ChatClient;
//using ChatMessage = JabbR_Core.Models.ChatMessage;
//using ChatRoom = JabbR_Core.Models.ChatRoom;
//using ChatUser = JabbR_Core.Models.ChatUser;
//using Notification = JabbR_Core.Models.Notification;
//using Settings = JabbR_Core.Models.Settings;

namespace JabbR_Core.Services
{
    public interface IJabbrRepository : IDisposable
    {
        IQueryable<ChatRoom> Rooms { get; }
        IQueryable<ChatUser> Users { get; }
        IQueryable<ChatClient> Clients { get; }
        IQueryable<Settings> Settings { get; }

        IQueryable<ChatUser> GetOnlineUsers(ChatRoom room);
        IQueryable<ChatUser> GetOnlineUsers();

        IQueryable<ChatUser> SearchUsers(string name);
        IQueryable<ChatMessage> GetMessagesByRoom(ChatRoom room);
        IQueryable<ChatMessage> GetPreviousMessages(string messageId);
        IQueryable<ChatRoom> GetAllowedRooms(ChatUser user);
        IQueryable<Notification> GetNotificationsByUser(ChatUser user);
        ChatMessage GetMessageById(string id);

        ChatUser GetUserById(string userId);
        ChatRoom GetRoomByName(string roomName);

        ChatUser GetUserByName(string userName);
        ChatUser GetUserByClientId(string clientId);
        ChatUser GetUserByLegacyIdentity(string userIdentity);
        ChatUser GetUserByIdentity(string providerName, string userIdentity);
        ChatUser GetUserByRequestResetPasswordId(string userName, string requestResetPasswordId);
        Notification GetNotificationById(int notificationId);

        ChatClient GetClientById(string clientId, bool includeUser = false);

        void AddUserRoom(ChatUser user, ChatRoom room);
        void RemoveUserRoom(ChatUser user, ChatRoom room);

        void Add(ChatClient client);
        void Add(ChatMessage message);
        void Add(ChatRoom room);
        void Add(ChatUser user);
        void Add(ChatUserIdentity identity);
        void Add(Attachment attachment);
        void Add(Settings settings);

        void Remove(ChatClient client);
        void Remove(ChatRoom room);
        void Remove(ChatUser user);
        void Remove(ChatUserIdentity identity);
        void CommitChanges();

        bool IsUserInRoom(ChatUser user, ChatRoom room);

        // Reload entities from the store
        void Reload(object entity);

        void Add(Notification notification);
        void Remove(Notification notification);
    }
}