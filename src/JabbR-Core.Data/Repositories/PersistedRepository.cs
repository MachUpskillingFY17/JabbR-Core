using System;
using System.Linq;
using JabbR_Core.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Data.Repositories
{
    public class PersistedRepository : IJabbrRepository
    {
        private readonly JabbrContext _db;

        private static readonly Func<JabbrContext, string, ChatUser> getUserByName = (db, userName) => db.AspNetUsers.FirstOrDefault(u => u.Name == userName);
        private static readonly Func<JabbrContext, string, ChatUser> getUserById = (db, userId) => db.AspNetUsers.Include(u=> u.OwnedRooms).FirstOrDefault(u => u.Id == userId);
        private static readonly Func<JabbrContext, string, string, ChatUserIdentity> getIdentityByIdentity = (db, providerName, userIdentity) => db.ChatUserIdentities.Include(i => i.UserKeyNavigation).FirstOrDefault(u => u.Identity == userIdentity && u.ProviderName == providerName);
        private static readonly Func<JabbrContext, string, ChatRoom> getRoomByName = (db, roomName) => db.ChatRooms.FirstOrDefault(r => r.Name == roomName);
        private static readonly Func<JabbrContext, string, ChatClient> getClientById = (db, clientId) => db.ChatClients.FirstOrDefault(c => c.Id == clientId);
        private static readonly Func<JabbrContext, string, ChatClient> getClientByIdWithUser = (db, clientId) => db.ChatClients.Include(c => c.UserKeyNavigation).FirstOrDefault(u => u.Id == clientId);
        private static readonly Func<JabbrContext, string, string, DateTimeOffset, ChatUser> getUserByRequestResetPasswordId = (db, userName, requestId, now) => db.AspNetUsers.FirstOrDefault(u => u.Name == userName && u.RequestPasswordResetId != null && u.RequestPasswordResetId.Equals(requestId, StringComparison.OrdinalIgnoreCase) && u.RequestPasswordResetValidThrough > now);

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
            get { return _db.AspNetUsers; }
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
            _db.AspNetUsers.Add(user);
            _db.SaveChanges();
        }

        public void Add(Attachment attachment)
        {
            _db.Attachments.Add(attachment);
            _db.SaveChanges();
        }

        public void Add(ChatUserIdentity identity)
        {
            _db.ChatUserIdentities.Add(identity);
            _db.SaveChanges();
        }

        public void Add(ChatMessage message)
        {
            _db.ChatMessages.Add(message);
            _db.SaveChanges();
        }

        public void Add(Notification notification)
        {
            _db.Notifications.Add(notification);
            _db.SaveChanges();
        }

        public void Add(ChatRoomOwners owner)
        {
            _db.ChatRoomOwners.Add(owner);


            _db.SaveChanges();
            
        }
        public void Add(ChatPrivateRoomUsers allowed)
        {
            _db.ChatPrivateRoomUsers.Add(allowed);
            _db.SaveChanges();
        }
        public void Add(ChatRoomUsers userRoom)
        {
            _db.ChatRoomUsers.Add(userRoom);
            _db.SaveChanges();
        }

        public void Remove(ChatRoom room)
        {
            _db.ChatRooms.Remove(room);
            _db.SaveChanges();
        }

        public void Remove(ChatUser user)
        {
            _db.AspNetUsers.Remove(user);
            _db.SaveChanges();
        }

        public void Remove(ChatUserIdentity identity)
        {
            _db.ChatUserIdentities.Remove(identity);
            _db.SaveChanges();
        }

        public void Remove(Notification notification)
        {
            _db.Notifications.Remove(notification);
            _db.SaveChanges();
        }

        //public void Remove(ChatMessage message)
        //{
        //    _db.ChatMessages.Remove(message);
        //    _db.SaveChanges();
        //}
        //public void Remove(Settings settings)
        //{
        //    _db.Settings.Remove(settings);
        //    _db.SaveChanges();
        //}

        public void Remove(ChatRoomOwners owner)
        {
            _db.ChatRoomOwners.Remove(owner);
            _db.SaveChanges();
        }
        public void Remove(ChatPrivateRoomUsers allowed)
        {
            _db.ChatPrivateRoomUsers.Remove(allowed);
            _db.SaveChanges();
        }
        public void Remove(ChatRoomUsers userRoom)
        {
            _db.ChatRoomUsers.Remove(userRoom);
            _db.SaveChanges();
        }

        public void CommitChanges()
        {
            _db.SaveChanges();
        }

        public void Dispose()
        {
            //_db.Dispose();
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
        public ChatRoom GetRoomById(int key)
        {
            return _db.ChatRooms.FirstOrDefault(r => r.Key == key);
        }

        public ChatMessage GetMessageById(string id)
        {
            return _db.ChatMessages.FirstOrDefault(m => m.Id == id);
        }

        public IQueryable<ChatRoom> GetAllowedRooms(ChatUser user)
        {
            // All public and private rooms the user can see.
            return _db.ChatRooms
                .Where(r =>
                       (!r.Private) ||
                       (r.Private && r.AllowedUsers.Any(u => u.ChatUserId == user.Id)));
        }

        public IQueryable<ChatRoom> GetOwnedRooms(ChatUser user)
        {
            var rooms = _db.ChatRoomOwners
                .Where(r => r.ChatUserId == user.Id)
                .Select(r => r.ChatRoomKeyNavigation);

            return rooms;
        }

        public IQueryable<ChatUser> GetRoomOwners(ChatRoom room)
        {
            var owners = _db.ChatRoomOwners
                .Where(r => r.ChatRoomKey == room.Key)
                .Select(r => r.ChatUserKeyNavigation);

            return owners;
        }

        public IQueryable<Notification> GetNotificationsByUser(ChatUser user)
        {
            return _db.Notifications.Include(n => n.RoomKeyNavigation)
                                    .Include(n => n.MessageKeyNavigation)
                                    .Include(n => n.MessageKeyNavigation.UserKeyNavigation)
                                    .Where(n => n.UserId == user.Id);
        }

        private IQueryable<ChatMessage> GetMessagesByRoom(string roomName)
        {
            return _db.ChatMessages.Include(r => r.RoomKeyNavigation).Where(r => r.RoomKeyNavigation.Name == roomName);
        }

        public IQueryable<ChatMessage> GetMessagesByRoom(ChatRoom room)
        {
            return _db.ChatMessages.Include(m => m.UserKeyNavigation)
                               .Include(m => m.RoomKeyNavigation)
                               .Where(m => m.RoomKey == room.Key);
        }

        public IQueryable<ChatMessage> GetPreviousMessages(string messageId)
        {
            var info = (from m in _db.ChatMessages.Include(m => m.RoomKeyNavigation)
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
            return _db.Entry(room)
                      .Collection(r => r.Users)
                      .Query()
                      .Online();
        }

        public IQueryable<ChatUser> GetOnlineUsers()
        {
            return _db.AspNetUsers.Include(c => c.ConnectedClients).Online();
        }

        public IQueryable<ChatUser> SearchUsers(string name)
        {
            return _db.AspNetUsers.Online().Where(u => u.Name.Contains(name));
        }

        public void AddUserRoom(ChatUser user, ChatRoom room)
        {
            // First, create a ChatRoomUsers object to represent this relationship
            ChatRoomUsers userroom = new ChatRoomUsers()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            // Add the relationship to the room's user list
            room.Users.Add(userroom);
            user.Rooms.Add(userroom);

            // Update the DB
            _db.Add(userroom);
            _db.SaveChanges();
        }

        public void RemoveUserRoom(ChatUser user, ChatRoom room)
        {
            // JC: First, find the ChatRoomUsers object that represents this relationship
            var chatUserChatRoom = from r in _db.ChatRoomUsers
                                   where (r.ChatRoomKey == room.Key) && (r.ChatUserId == user.Id)
                                   select r;

            // We found the correct relationship
            if (chatUserChatRoom.Count() == 1)
            {
                // Remove this object from the room's Users list
                // We can use .First() becasue the ChatRoomKey and ChatUserKey are primary keys and combined they will only return one unique value
                room.Users.Remove(chatUserChatRoom.First());
                user.Rooms.Remove(chatUserChatRoom.First());

                // Now delete the relationship object
                _db.Remove(chatUserChatRoom.First());
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
            return _db.AspNetUsers.FirstOrDefault(u => u.Identity == userIdentity);
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