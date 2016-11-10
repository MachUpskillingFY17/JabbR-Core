using System;
using System.Linq;
using JabbR_Core.ViewModels;
using JabbR_Core.Data.Models;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;

namespace JabbR_Core.Services
{
    public class InMemoryRepository : IJabbrRepository
    {
        public List<string> ChatRooms { get; set; }
        public string RoomNames { get; set; }
        public UserViewModel UserModel { get; set; }
        public ChatClient ChatClient { get; set; }
        public ClientState ClientState { get; set; }

        // Mock List for LoadRooms()
       
        //public List<ChatRoom> RoomList { get; set; }

        // Mock List for GetRoom()
        public List<LobbyRoomViewModel> LobbyRoomList { get; set; }
        public LobbyRoomViewModel LobbyRoomView { get; set; }
        public RoomViewModel RoomViewModel { get; set; }


        private readonly ICollection<ChatUser> _users;
        private readonly ICollection<ChatRoom> _rooms;

        private readonly ICollection<Settings> _settings;
        private readonly ICollection<Attachment> _attachments;
        private readonly ICollection<Notification> _notifications;
        private readonly ICollection<ChatPrivateRoomUsers> _allowed;
        private readonly ICollection<ChatRoomOwners> _owner;
        private readonly ICollection<ChatRoomUsers> _userRooms;
        private readonly ICollection<ChatUserIdentity> _identities;
            
        public InMemoryRepository(JabbrContext context)
        {
            /*AJS: UNCOMMENTED THIS AND COMMENTED OUT HARD CODING TO GET TESTS TO WORK*/
            _users = new SafeCollection<ChatUser>();
            _rooms = new SafeCollection<ChatRoom>();
            _identities = new SafeCollection<ChatUserIdentity>();
            _attachments = new SafeCollection<Attachment>();
            _notifications = new SafeCollection<Notification>();
            _settings = new SafeCollection<Settings>();
            _allowed = new SafeCollection<ChatPrivateRoomUsers>();
            _owner = new SafeCollection<ChatRoomOwners>();
            _userRooms = new SafeCollection<ChatRoomUsers>();

            var user = new ChatUser
            {
                Id = "1",
                Name = "Jane",
                LastActivity = Convert.ToDateTime("2016-08-23 00:26:35.713"),
                IsAdmin = true,
                IsAfk = true,
                Status = 1
            };
            _users.Add(user);


            ChatClient = new ChatClient
            {
                Key = 1,
                Id = "meow",
                Name = "testClient",
            };

            ClientState = new ClientState
            {
                ActiveRoom = "Lobby"
            };

            // instantiate UserViewModel object from User
            UserModel = new UserViewModel(user);


            // Add RoomView to RoomList

            LobbyRoomList = new List<LobbyRoomViewModel> { };
        }

        public ChatRoom GetRoomById(int key) { return new ChatRoom(); }
        public IQueryable<ChatRoom> Rooms { get { return _rooms.AsQueryable(); } }

        public IQueryable<ChatUser> Users { get { return _users.AsQueryable(); } }

        public IQueryable<ChatClient> Clients { get { return _users.SelectMany(u => u.ConnectedClients).AsQueryable(); } }
        //public IQueryable<ChatClient> Clients
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
        public IQueryable<Settings> Settings { get { return _settings.AsQueryable(); } }

        

        public void Add(Attachment attachment)
        {
            _attachments.Add(attachment);
        }

        public void Add(ChatRoom room)
        {
            //RoomList.Add(room);
            _rooms.Add(room);
        }

        public void Add(ChatUser user)
        {
            _users.Add(user);
        }

        public void Add(ChatUserIdentity identity)
        {
            _identities.Add(identity);
            if (identity.UserKeyNavigation != null)
            {
                //identity.User.Identities.Add(identity);
            }
        }

        public void Add(Settings settings)
        {
            _settings.Add(settings);
        }

        public void Add(ChatMessage message)
        {
            // There's no need to keep a collection of messages outside of a room
            var room = _rooms.First(r => r == message.RoomKeyNavigation);
            room.ChatMessages.Add(message);
        }

        public void Add(ChatClient client)
        {
            var user = _users.FirstOrDefault(u => client.UserKeyNavigation == u);
            user.ConnectedClients.Add(client);
        }

        public void Add(Notification notification)
        {
            _notifications.Add(notification);
        }

        public void Add(ChatRoomOwners owner)
        {
            _owner.Add(owner);
        }
        public void Add(ChatPrivateRoomUsers allowed)
        {
            _allowed.Add(allowed);
        }
        public void Add(ChatRoomUsers userRoom)
        {
            _userRooms.Add(userRoom); 
        }

        public void Remove(ChatClient client)
        {
            var user = _users.FirstOrDefault(u => client.UserKeyNavigation == u);
            user.ConnectedClients.Remove(client);
        }

        public void Remove(ChatRoom room)
        {
            _rooms.Remove(room);
        }

        public void Remove(ChatUser user)
        {
            _users.Remove(user);
        }

        public void Remove(ChatUserIdentity identity)
        {
            _identities.Remove(identity);
        }

        public void Remove(Notification notification)
        {
            _notifications.Remove(notification);
        }

        public void Remove(ChatRoomOwners owner)
        {
            _owner.Remove(owner);
        }
        public void Remove(ChatPrivateRoomUsers allowed)
        {
            _allowed.Remove(allowed);
        }
        public void Remove(ChatRoomUsers userRoom)
        {
            _userRooms.Remove(userRoom);
        }

        public void CommitChanges()
        {
            // no-op since this is an in-memory impl' of the repo
        }

        public void Dispose()
        {
        }

        public ChatUser GetUserById(string userId)
        {
            return _users.FirstOrDefault(u => u.Id != null && u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
        }

        public ChatUser GetUserByName(string userName)
        {
            return _users.FirstOrDefault(u => u.Name != null && u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }

        public ChatRoom GetRoomByName(string roomName)
        {
            return _rooms.FirstOrDefault(r => r.Name != null && r.Name.Equals(roomName, StringComparison.OrdinalIgnoreCase));
        }

        public ChatRoom GetRoomByName(string roomName, bool includeUsers = false, bool includeOwners = false)
        {
            return GetRoomByName(roomName);
        }

        public ChatRoom GetRoomAndUsersByName(string roomName)
        {
            return GetRoomByName(roomName);
        }

        public IQueryable<ChatPrivateRoomUsers> GetAllowedRooms(ChatUser user)
        {
            var allowedRooms = _rooms.Allowed(user.Id);

            return null;
        }

        public IQueryable<ChatRoomOwners> GetOwnedRooms(ChatUser user)
        {
            var rooms = _owner
                .Where(r => r.ChatUserId == user.Id)
                .AsQueryable();

            return rooms;
        }

        public IQueryable<ChatUser> GetRoomOwners(ChatRoom room)
        {
            var owners = _owner
                .Where(r => r.ChatRoomKey == room.Key)
                .Select(r => r.ChatUserKeyNavigation)
                .AsQueryable();

            return owners;
        }

        public IQueryable<Notification> GetNotificationsByUser(ChatUser user)
        {
            return _notifications.Where(n => n.UserId == user.Id).AsQueryable();
        }

        public IQueryable<ChatMessage> GetMessagesByRoom(ChatRoom room)
        {
            return room.ChatMessages.AsQueryable();
        }

        public IQueryable<ChatUser> GetOnlineUsers(ChatRoom room)
        {
            return room.Users.Online().AsQueryable();
        }

        public IQueryable<ChatUser> GetOnlineUsers()
        {
            return _users.Online().AsQueryable();
        }

        public IQueryable<ChatUser> SearchUsers(string name)
        {
            return _users.Online()
                         .Where(u => u.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1)
                         .AsQueryable();
        }

        public ChatUser GetUserByClientId(string clientId)
        {
            return _users.FirstOrDefault(u => u.ConnectedClients.Any(c => c.Id == clientId));
        }

        public ChatUser GetUserByLegacyIdentity(string userIdentity)
        {
            return _users.FirstOrDefault(u => u.Identity == userIdentity);
        }

        public ChatUser GetUserByIdentity(string providerName, string userIdentity)
        {
            var identity = _identities.FirstOrDefault(u => u.Identity == userIdentity && u.ProviderName == providerName);
            if (identity != null)
            {
                return identity.UserKeyNavigation;
            }
            return null;
        }

        public ChatUser GetUserByRequestResetPasswordId(string userName, string requestResetPasswordId)
        {
            return _users.FirstOrDefault(u => u.RequestPasswordResetId != null &&
                                              u.RequestPasswordResetId.Equals(requestResetPasswordId, StringComparison.OrdinalIgnoreCase) &&
                                              u.RequestPasswordResetValidThrough > DateTimeOffset.UtcNow);
        }

        public Notification GetNotificationById(int notificationId)
        {
            return _notifications.SingleOrDefault(n => n.Key == notificationId);
        }

        public ChatClient GetClientById(string clientId, bool includeUser = false)
        {
            return _users.SelectMany(u => u.ConnectedClients).FirstOrDefault(c => c.Id == clientId);
        }

        public IQueryable<ChatUser> GetUsersByRoom(ChatRoom room)
        {
            /*var users = _db.ChatRoomUsers
                    .Where(r => r.ChatRoomKey == room.Key)
                    .Select(r => r.ChatUserKeyNavigation);*/

            return null;
        }

        public IQueryable<ChatMessage> GetPreviousMessages(string messageId)
        {
            // Ineffcient since we don't have a messages collection

            return (from r in _rooms
                    let message = r.ChatMessages.FirstOrDefault(m => m.Id == messageId)
                    where message != null
                    from m in r.ChatMessages
                    where m.When < message.When
                    select m).AsQueryable();
        }

        public ChatMessage GetMessageById(string id)
        {
            return (from r in _rooms
                    let message = r.ChatMessages.FirstOrDefault(m => m.Id == id)
                    where message != null
                    select message).FirstOrDefault();
        }

        public bool IsUserInRoom(ChatUser user, ChatRoom room)
        {
            // REVIEW: Inefficient, bu only users for unit tests right now
            return room.Users.Any(u => u.ChatUserKeyNavigation.Name == user.Name);
        }

        public void AddUserRoom(ChatUser user, ChatRoom room)
        {
            // Create new user room relationship
            var userRoom = new ChatRoomUsers()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            // Add relationship to both user and room
            user.Rooms.Add(userRoom);
            room.Users.Add(userRoom);
        }

        public void RemoveUserRoom(ChatUser user, ChatRoom room)
        {
            // First find the correct relationship in the user and the room
            var userRelation = user.Rooms.ToList().Find(rm => rm.ChatRoomKeyNavigation == room);
            var roomRelation = room.Users.ToList().Find(rm => rm.ChatUserKeyNavigation == user);

            // This will either find 1 or 0 results, so we can remove the first result from each list
            user.Rooms.Remove(userRelation);
            room.Users.Remove(roomRelation);
        }

        public void Reload(object entity)
        {
        }

        IQueryable<ChatRoomOwners> IJabbrRepository.GetOwnedRooms(ChatUser user)
        {
            throw new NotImplementedException();
        }
    }
}