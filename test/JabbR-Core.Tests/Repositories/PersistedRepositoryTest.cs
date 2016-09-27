using Xunit;
using System;
using System.Linq;
using JabbR_Core.Data.Models;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JabbR_Core.Tests.Repositories
{
    public class PersistedRepositoryTest
    {
        JabbrContext _context;
        PersistedRepository _repository;
        //DbContextOptionsBuilder _options;
        DbContextOptions<JabbrContext> _options;

        public PersistedRepositoryTest()
        {
            //IServiceCollection service = new ServiceCollection();

            //_options = new DbContextOptionsBuilder();
            _options = new DbContextOptions<JabbrContext>();

            /*string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JabbREFTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            service.AddDbContext<JabbrContext>(_options => _options.UseSqlServer(connection));

            var contextOps = new DbContextOptions<JabbrContext>();*/

            //_context = new JabbrContext(contextOps);
            _context = new JabbrContext(_options);


            _repository = new PersistedRepository(_context);
        }

        [Fact]
        public void AddUser()
        {
            // Create a new chat user
            var user1 = new ChatUser()
            {
                Id = "1",
                Name = "User 1",
                LastActivity = DateTime.Now
            };

            // Try to add the user to the repository
            _repository.Add(user1);

            // Make sure repository returns the correct information
            var user = _repository.Users.First();

            Assert.Equal(user1, user);

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddUser: Complete");
        }

        [Fact]
        public void AddRoom()
        {
            // Create a user to populate the Creator_Key attribute in ChatRoom
            var user1 = new ChatUser()
            {
                Id = "2",
                Name = "User 1",
                LastActivity = DateTime.Now
            };
            _repository.Add(user1);

            // Create a new chat room
            var creatorKey = _repository.Users.First().Key;
            var room1 = new ChatRoom()
            {
                Name = "Room 1",
                Closed = false,
                Topic = "Horses",
                Creator_Key = creatorKey
            };

            // Try to add the room to the repository
            _repository.Add(room1);

            // Make sure repository returns the correct information
            var room = _repository.Rooms.First();

            Assert.Equal(room1, room);

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            _repository.Remove(room1);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddRoom: Complete");
        }

        [Fact]
        public void AddClient()
        {
            // Create a user to populate the UserKey attribute in ChatClient
            var user1 = new ChatUser()
            {
                Id = "3",
                Name = "User 1",
                LastActivity = DateTime.Now
            };
            _repository.Add(user1);

            // Create a new client
            var userKey = _repository.Users.First().Key;
            var client1 = new ChatClient()
            {
                Id = "1",
                LastActivity = DateTime.Now,
                LastClientActivity = DateTime.Now,
                UserKey = userKey
            };

            // Try to add the client to the repository
            _repository.Add(client1);

            // Make sure repository returns the correct information
            var client = _repository.Clients.First();

            Assert.Equal(client1, client);

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            _repository.Remove(client1);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddClient: Complete");
        }

        /*public IQueryable<Settings> Settings
        {
            get
            {
                throw new NotImplementedException();
            }
        }*/

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
