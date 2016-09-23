using Xunit;
using System;
using System.Linq;
using JabbR_Core.Data.Models;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JabbR_Core.Tests.Repositories
{
    public class PersistedRepositoryTest
    {
        JabbrContext _context;
        PersistedRepository _repository;
        DbContextOptions<JabbrContext> _options;
        DbContextOptionsBuilder _builder;

        public PersistedRepositoryTest()
        {
            _options = new DbContextOptions<JabbrContext>();            
            _context = new JabbrContext(_options);
            _repository = new PersistedRepository(_context);
        }

      [Fact]
        public void GetRooms()
        {
            // Create two new chat rooms
            var room1 = new ChatRoom()
            {
                Key = 1,
                Name = "Room 1",
                Topic = "Horses"
            };

            var room2 = new ChatRoom()
            {
                Key = 2,
                Name = "Room 2",
                Topic = "Poetry"
            };

            var testRooms = new List<ChatRoom>() { room1, room2};

            // Populate DB with new chat rooms
            _context.Add(room1);
            _context.Add(room2);

            // Make sure repository returns the correct information
            var rooms = _repository.Rooms.ToList();

            Assert.Equal(testRooms, rooms);

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            _context.Remove(room1);
            _context.Remove(room2);
        }

        [Fact]
        public void GetUsers()
        {
            // Create two new chat users
            var user1 = new ChatUser()
            {
                Id = "1",
                Name = "User 1",
                LastActivity = DateTime.Now
            };

            var user2 = new ChatUser()
            {
                Id = "2",
                Name = "User 2",
                LastActivity = DateTime.Now
            };

            var testUsers = new List<ChatUser>() { user1, user2 };

            // Populate DB with new chat users
            _context.Add(user1);
            _context.Add(user2);

            // Make sure repository returns the correct information
            var users = _repository.Users.ToList();

            Assert.Equal(testUsers, users);

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            _context.Remove(user1);
        }

        public IQueryable<ChatClient> Clients
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IQueryable<Settings> Settings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IQueryable<ChatUser> GetOnlineUsers(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatUser> GetOnlineUsers()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatUser> SearchUsers(string name)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatMessage> GetMessagesByRoom(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatMessage> GetPreviousMessages(string messageId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatRoom> GetAllowedRooms(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Notification> GetNotificationsByUser(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public ChatMessage GetMessageById(string id)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public ChatRoom GetRoomByName(string roomName)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByName(string userName)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByClientId(string clientId)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByLegacyIdentity(string userIdentity)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByIdentity(string providerName, string userIdentity)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByRequestResetPasswordId(string userName, string requestResetPasswordId)
        {
            throw new NotImplementedException();
        }

        public Notification GetNotificationById(int notificationId)
        {
            throw new NotImplementedException();
        }

        public ChatClient GetClientById(string clientId, bool includeUser = false)
        {
            throw new NotImplementedException();
        }

        public void AddUserRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatClient client)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatMessage message)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatUserIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void Add(Attachment attachment)
        {
            throw new NotImplementedException();
        }

        public void Add(Settings settings)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatRoomChatUserOwner owner)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatRoomChatUserAllowed allowed)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatUserChatRooms userRoom)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatClient client)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatUserIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatRoomChatUserOwner owner)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatRoomChatUserAllowed allowed)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatUserChatRooms userRoom)
        {
            throw new NotImplementedException();
        }

        public void CommitChanges()
        {
            throw new NotImplementedException();
        }

        public bool IsUserInRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Reload(object entity)
        {
            throw new NotImplementedException();
        }

        public void Add(Notification notification)
        {
            throw new NotImplementedException();
        }

        public void Remove(Notification notification)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
