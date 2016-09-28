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
        public ChatUser User { get; set; }
        public string RoomNames { get; set; }
        public UserViewModel UserModel { get; set; }
        public ChatClient ChatClient { get; set; }
        public ClientState ClientState { get; set; }

        // Mock List for LoadRooms()
        public ChatRoom Room { get; set; }
        public List<ChatRoom> RoomList { get; set; }

        // Mock List for GetRoom()
        public List<LobbyRoomViewModel> LobbyRoomList { get; set; }
        public LobbyRoomViewModel LobbyRoomView { get; set; }
        public RoomViewModel RoomViewModel { get; set; }


        private readonly ICollection<ChatUser> _users;
        private readonly ICollection<ChatUserIdentity> _identities;
        private readonly ICollection<ChatRoom> _rooms;
        private readonly ICollection<Attachment> _attachments;
        private readonly ICollection<Notification> _notifications;
        private readonly ICollection<Settings> _settings;
        private readonly ICollection<ChatRoomChatUserAllowed> _allowed;
        private readonly ICollection<ChatRoomChatUserOwner> _owner;
        private readonly ICollection<ChatUserChatRooms> _userRooms;

        public InMemoryRepository()
        {
            /*AJS: UNCOMMENTED THIS AND COMMENTED OUT HARD CODING TO GET TESTS TO WORK*/
            _users = new SafeCollection<ChatUser>();
            _rooms = new SafeCollection<ChatRoom>();
            _identities = new SafeCollection<ChatUserIdentity>();
            _attachments = new SafeCollection<Attachment>();
            _notifications = new SafeCollection<Notification>();
            _settings = new SafeCollection<Settings>();
            _allowed = new SafeCollection<ChatRoomChatUserAllowed>();
            _owner = new SafeCollection<ChatRoomChatUserOwner>();
            _userRooms = new SafeCollection<ChatUserChatRooms>();

            /*User = new ChatUser
            {
                Id= "1",
                Name = "user1",
                LastActivity = Convert.ToDateTime("2016-08-23 00:26:35.713"),
                IsAdmin = true,
                IsAfk = true
            };

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
            UserModel = new UserViewModel(User);

            // populate ChatRoom and RoomList
            Room = new ChatRoom { Name = "light_meow" };
            RoomList = new List<ChatRoom> { Room };

            RoomViewModel = new RoomViewModel();

            // populate RoomView
            LobbyRoomView = new LobbyRoomViewModel
            {
                Name = Room.Name,
                Count = 1,
                Topic = "jabbr"
            };
            // Add RoomView to RoomList
            LobbyRoomList = new List<LobbyRoomViewModel> {  };*/


        }

        public IQueryable<ChatRoom> Rooms { get { return _rooms.AsQueryable(); } }

        public IQueryable<ChatUser> Users { get { return _users.AsQueryable(); } }

        //public IQueryable<ChatClient> Clients { get { return _users.SelectMany(u => u.ConnectedClients).AsQueryable(); } }
        public IQueryable<ChatClient> Clients
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public IQueryable<Settings> Settings { get { return _settings.AsQueryable(); } }

        

        public void Add(Attachment attachment)
        {
            _attachments.Add(attachment);
        }

        public void Add(ChatRoom room)
        {
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
            //room.Messages.Add(message);
        }

        public void Add(ChatClient client)
        {
            var user = _users.FirstOrDefault(u => client.UserKeyNavigation == u);
            //user.ConnectedClients.Add(client);
        }

        public void Add(Notification notification)
        {
            _notifications.Add(notification);
        }

        public void Add(ChatRoomChatUserOwner owner)
        {
            _owner.Add(owner);
        }
        public void Add(ChatRoomChatUserAllowed allowed)
        {
            _allowed.Add(allowed);
        }
        public void Add(ChatUserChatRooms userRoom)
        {
            _userRooms.Add(userRoom); 
        }

        public void Remove(ChatClient client)
        {
            var user = _users.FirstOrDefault(u => client.UserKeyNavigation == u);
            //user.ConnectedClients.Remove(client);
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

        public void Remove(ChatRoomChatUserOwner owner)
        {
            _owner.Remove(owner);
        }
        public void Remove(ChatRoomChatUserAllowed allowed)
        {
            _allowed.Remove(allowed);
        }
        public void Remove(ChatUserChatRooms userRoom)
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

        public IQueryable<ChatRoom> GetAllowedRooms(ChatUser user)
        {
            var allowedRooms = _rooms.Allowed(user.Id);

            return _rooms
                .Where(r =>
                    (!r.Private) ||
                    (r.Private && allowedRooms.Contains(r)))
                .AsQueryable();
        }

        public IQueryable<Notification> GetNotificationsByUser(ChatUser user)
        {
            return _notifications.Where(n => n.UserKey == user.Key).AsQueryable();
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
            var userRoom = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
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
            var userRelation= user.Rooms.Where(r => (r.ChatRoomKey == room.Key) && (r.ChatUserKey == user.Key));
            var roomRelation = room.Users.Where(r => (r.ChatRoomKey == room.Key) && (r.ChatUserKey == user.Key));

            // This will either find 1 or 0 results, so we can remove the first result from each list
            user.Rooms.Remove(userRelation.First());
            room.Users.Remove(roomRelation.First());
        }

        public void Reload(object entity)
        {
        }
        
    }
}