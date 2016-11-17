using System;
using System.Linq;
using JabbR_Core.Data.Models;

namespace JabbR_Core.Data.Repositories
{
    public interface IJabbrRepository
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
        IQueryable<ChatPrivateRoomUsers> GetAllowedRooms(ChatUser user);
        IQueryable<ChatRoomOwners> GetOwnedRooms(ChatUser user);
        IQueryable<ChatUser> GetRoomOwners(ChatRoom room);
        IQueryable<Notification> GetNotificationsByUser(ChatUser user);
        
        ChatMessage GetMessageById(string id);
        ChatUser GetUserById(string userId);
        ChatRoom GetRoomByName(string roomName);
        ChatRoom GetRoomById(int key);
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
        void Add(ChatRoomOwners owner);
        void Add(ChatPrivateRoomUsers allowed);
        void Add(ChatRoomUsers userRoom);

        void Remove(ChatClient client);
        void Remove(ChatRoom room);
        void Remove(ChatUser user);
        void Remove(ChatUserIdentity identity);
        void Remove(ChatRoomOwners owner);
        void Remove(ChatPrivateRoomUsers allowed);
        void Remove(ChatRoomUsers userRoom);
        void CommitChanges();

        bool IsUserInRoom(ChatUser user, ChatRoom room);

        // Reload entities from the store
        void Reload(object entity);

        void Add(Notification notification);
        void Remove(Notification notification);
    }
}