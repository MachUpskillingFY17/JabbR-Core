using System;
using System.Linq;
using JabbR_Core.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Data.Repositories
{
    public class PersistedRepository : IJabbrRepository
    {
        private readonly JabbrContext _db;

        private static readonly Func<JabbrContext, string, ChatUser> getUserByName = (db, userName) => db.ChatUsers.FirstOrDefault(u => u.Name == userName);
        private static readonly Func<JabbrContext, string, ChatUser> getUserById = (db, userId) => db.ChatUsers.FirstOrDefault(u => u.Id == userId);
        private static readonly Func<JabbrContext, string, string, ChatUserIdentity> getIdentityByIdentity = (db, providerName, userIdentity) => db.Identities.Include(i => i.UserKeyNavigation).FirstOrDefault(u => u.Identity == userIdentity && u.ProviderName == providerName);
        private static readonly Func<JabbrContext, string, ChatRoom> getRoomByName = (db, roomName) => db.ChatRooms.FirstOrDefault(r => r.Name == roomName);
        private static readonly Func<JabbrContext, string, ChatClient> getClientById = (db, clientId) => db.ChatClients.FirstOrDefault(c => c.Id == clientId);
        private static readonly Func<JabbrContext, string, ChatClient> getClientByIdWithUser = (db, clientId) => db.ChatClients.Include(c => c.UserKeyNavigation).FirstOrDefault(u => u.Id == clientId);
        private static readonly Func<JabbrContext, string, string, DateTimeOffset, ChatUser> getUserByRequestResetPasswordId = (db, userName, requestId, now) => db.ChatUsers.FirstOrDefault(u => u.Name == userName && u.RequestPasswordResetId != null && u.RequestPasswordResetId.Equals(requestId, StringComparison.OrdinalIgnoreCase) && u.RequestPasswordResetValidThrough > now);

        public PersistedRepository(JabbrContext db)
        {
            _db = db;
        }

        public IQueryable<ChatRoom> Rooms
        {
            get { return _db.ChatRooms; }
        }

        public IQueryable<ChatUser> Users
        {
            get { return _db.ChatUsers; }
        }

        public IQueryable<ChatClient> Clients
        {
            get { return _db.ChatClients; }
        }
        public IQueryable<Settings> Settings
        {
            get { return _db.Settings; }
        }

        public void Add(Settings settings)
        {
            _db.Settings.Add(settings);
            _db.SaveChanges();
        }

        public void Add(ChatRoom room)
        {
            _db.ChatRooms.Add(room);
            _db.SaveChanges();
        }

        public void Add(ChatUser user)
        {
            _db.ChatUsers.Add(user);
            _db.SaveChanges();
        }

        public void Add(Attachment attachment)
        {
            _db.Attachments.Add(attachment);
            _db.SaveChanges();
        }

        public void Add(ChatUserIdentity identity)
        {
            _db.Identities.Add(identity);
            _db.SaveChanges();
        }

        public void Add(ChatMessage message)
        {
            _db.Messages.Add(message);
            _db.SaveChanges();
        }

        public void Add(Notification notification)
        {
            _db.Notifications.Add(notification);
            _db.SaveChanges();
        }

        public void Add(ChatRoomChatUserOwner owner)
        {
            _db.ChatRoomsChatUsersOwned.Add(owner);
            _db.SaveChanges();
        }
        public void Add(ChatRoomChatUserAllowed allowed)
        {
            _db.ChatRoomsChatUsersAllowed.Add(allowed);
            _db.SaveChanges();
        }
        public void Add(ChatUserChatRooms userRoom)
        {
            _db.ChatUserChatRooms.Add(userRoom);
            _db.SaveChanges();
        }

        public void Remove(ChatRoom room)
        {
            _db.ChatRooms.Remove(room);
            _db.SaveChanges();
        }

        public void Remove(ChatUser user)
        {
            _db.ChatUsers.Remove(user);
            _db.SaveChanges();
        }

        public void Remove(ChatUserIdentity identity)
        {
            _db.Identities.Remove(identity);
            _db.SaveChanges();
        }

        public void Remove(Notification notification)
        {
            _db.Notifications.Remove(notification);
            _db.SaveChanges();
        }

        public void Remove(Settings settings)
        {
            _db.Settings.Remove(settings);
            _db.SaveChanges();
        }

        public void Remove(ChatRoomChatUserOwner owner)
        {
            _db.ChatRoomsChatUsersOwned.Remove(owner);
            _db.SaveChanges();
        }
        public void Remove(ChatRoomChatUserAllowed allowed)
        {
            _db.ChatRoomsChatUsersAllowed.Remove(allowed);
            _db.SaveChanges();
        }
        public void Remove(ChatUserChatRooms userRoom)
        {
            _db.ChatUserChatRooms.Remove(userRoom);
            _db.SaveChanges();
        }

        public void CommitChanges()
        {
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public ChatUser GetUserById(string userId)
        {
            return getUserById(_db, userId);
        }

        public ChatUser GetUserByName(string userName)
        {
            return getUserByName(_db, userName);
        }

        public ChatRoom GetRoomByName(string roomName)
        {
            return getRoomByName(_db, roomName);
        }

        public ChatMessage GetMessageById(string id)
        {
            return _db.Messages.FirstOrDefault(m => m.Id == id);
        }

        public IQueryable<ChatRoom> GetAllowedRooms(ChatUser user)
        {
            // All public and private rooms the user can see.
            return _db.ChatRooms
                .Where(r =>
                       (!r.Private) ||
                       (r.Private && r.AllowedUsers.Any(u => u.ChatUserKey == user.Key)));
        }

        public IQueryable<Notification> GetNotificationsByUser(ChatUser user)
        {
            return _db.Notifications.Include(n => n.RoomKeyNavigation)
                                    .Include(n => n.MessageKeyNavigation)
                                    .Include(n => n.MessageKeyNavigation.UserKeyNavigation)
                                    .Where(n => n.UserKey == user.Key);
        }

        private IQueryable<ChatMessage> GetMessagesByRoom(string roomName)
        {
            return _db.Messages.Include(r => r.RoomKeyNavigation).Where(r => r.RoomKeyNavigation.Name == roomName);
        }

        public IQueryable<ChatMessage> GetMessagesByRoom(ChatRoom room)
        {
            return _db.Messages.Include(m => m.UserKeyNavigation)
                               .Include(m => m.RoomKeyNavigation)
                               .Where(m => m.RoomKey == room.Key);
        }

        public IQueryable<ChatMessage> GetPreviousMessages(string messageId)
        {
            var info = (from m in _db.Messages.Include(m => m.RoomKeyNavigation)
                        where m.Id == messageId
                        select new
                        {
                            m.When,
                            RoomName = m.RoomKeyNavigation.Name
                        }).FirstOrDefault();

            return from m in GetMessagesByRoom(info.RoomName)
                   where m.When < info.When
                   select m;
        }

        public IQueryable<ChatUser> GetOnlineUsers(ChatRoom room)
        {
            var temp = _db.Entry(room)
                      .Collection(r => r.Users)
                      .Query()
                      .Online();

            return temp;
        }

        public IQueryable<ChatUser> GetOnlineUsers()
        {
            return _db.ChatUsers.Include(c => c.ConnectedClients).Online();
        }

        public IQueryable<ChatUser> SearchUsers(string name)
        {
            return _db.ChatUsers.Online().Where(u => u.Name.Contains(name));
        }

        public void AddUserRoom(ChatUser user, ChatRoom room)
        {
            // First, create a ChatUserChatRooms object to represent this relationship
            ChatUserChatRooms userroom = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            // Add the relationship to the room's user list
            room.Users.Add(userroom);

            // Update the DB
            _db.Add(userroom);
            _db.SaveChanges();
        }

        public void RemoveUserRoom(ChatUser user, ChatRoom room)
        {
            // JC: First, find the ChatUserChatRooms object that represents this relationship
            var chatUserChatRoom = from r in _db.ChatUserChatRooms
                                   where (r.ChatRoomKey == room.Key) && (r.ChatUserKey == user.Key)
                                   select r;

            // We found the correct relationship
            if (chatUserChatRoom.Count() == 1)
            {
                // Remove this object from the room's Users list
                // We can use .First() becasue the ChatRoomKey and ChatUserKey are primary keys and combined they will only return one unique value
                room.Users.Remove(chatUserChatRoom.First());

                // Now delete the relationship object
                _db.Remove(chatUserChatRoom);
                _db.SaveChanges();
            }
        }

        public void Add(ChatClient client)
        {
            _db.ChatClients.Add(client);
            _db.SaveChanges();
        }

        public void Remove(ChatClient client)
        {
            _db.ChatClients.Remove(client);
            _db.SaveChanges();
        }

        public ChatUser GetUserByClientId(string clientId)
        {
            var client = GetClientById(clientId, includeUser: true);
            if (client != null)
            {
                return client.UserKeyNavigation;
            }
            return null;
        }

        public ChatUser GetUserByIdentity(string providerName, string userIdentity)
        {
            ChatUserIdentity identity = getIdentityByIdentity(_db, providerName, userIdentity);
            if (identity != null)
            {
                return identity.UserKeyNavigation;
            }
            return null;
        }

        public ChatUser GetUserByRequestResetPasswordId(string userName, string requestResetPasswordId)
        {
            return getUserByRequestResetPasswordId(_db, userName, requestResetPasswordId, DateTimeOffset.UtcNow);
        }

        public Notification GetNotificationById(int notificationId)
        {
            return _db.Notifications.SingleOrDefault(n => n.Key == notificationId);
        }

        public ChatUser GetUserByLegacyIdentity(string userIdentity)
        {
            return _db.ChatUsers.FirstOrDefault(u => u.Identity == userIdentity);
        }

        public ChatClient GetClientById(string clientId, bool includeUser = false)
        {
            if (includeUser)
            {
                return getClientByIdWithUser(_db, clientId);
            }

            return getClientById(_db, clientId);
        }

        public bool IsUserInRoom(ChatUser user, ChatRoom room)
        {
            return _db.Entry(user)
                      .Collection(r => r.Rooms)
                      .Query()
                      .Where(r => r.ChatRoomKey == room.Key)
                      .Select(r => r.ChatRoomKeyNavigation.Name)
                      .FirstOrDefault() != null;
        }

        public void Reload(object entity)
        {
            _db.Entry(entity).Reload();
        }

    }
}