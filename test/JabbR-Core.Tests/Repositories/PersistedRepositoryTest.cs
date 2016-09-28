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
        public void AddAndRemoveUser()
        {
            // Create a new chat user
            var userExpected = new ChatUser()
            {
                Id = "1",
                Name = "User 1",
                LastActivity = DateTime.Now
            };

            // Try to add the user to the repository
            _repository.Add(userExpected);

            // Make sure repository returns the correct information
            Assert.Equal(userExpected, _repository.Users.First());

            // Clean up data
            _repository.Remove(userExpected);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveUser: Complete");
        }

        [Fact]
        public void AddAndRemoveRoom()
        {
            // Create a user to populate the Creator_Key attribute in ChatRoom
            var user = new ChatUser()
            {
                Id = "2",
                Name = "User 1",
                LastActivity = DateTime.Now
            };
            _repository.Add(user);

            // Create a new chat room
            var creatorKey = _repository.Users.First().Key;
            var roomExpected = new ChatRoom()
            {
                Name = "Room 1",
                Closed = false,
                Topic = "Horses",
                Creator_Key = creatorKey
            };

            // Try to add the room to the repository
            _repository.Add(roomExpected);

            // Make sure repository returns the correct information
            Assert.Equal(roomExpected, _repository.Rooms.First());
            Assert.Equal(roomExpected, _repository.GetRoomByName("Room 1"));

            // Clean up data
            _repository.Remove(roomExpected);
            _repository.Remove(user);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveRoom: Complete");
        }

        [Fact]
        public void AddAndRemoveClient()
        {
            // Create a user to populate the UserKey attribute in ChatClient
            var user = new ChatUser()
            {
                Id = "3",
                Name = "User 1",
                LastActivity = DateTime.Now
            };
            _repository.Add(user);

            // Create a new client
            var userKey = _repository.Users.First().Key;
            var clientExpected = new ChatClient()
            {
                Id = "1",
                LastActivity = DateTime.Now,
                LastClientActivity = DateTime.Now,
                UserKey = userKey
            };
            _repository.Add(clientExpected);

            // Make sure repository returns the correct information
            Assert.Equal(clientExpected, _repository.Clients.First());
            Assert.Equal(clientExpected, _repository.GetClientById("1"));

            // Clean up data
            _repository.Remove(clientExpected);
            _repository.Remove(user);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveClient: Complete");
        }

        [Fact]
        public void AddAndRemoveSettings()
        {
            // Create new settings
            var settingsExpected = new Settings()
            {
                RawSettings = "These are my test settings."
            };

            // Try to add the settings to the repository
            _repository.Add(settingsExpected);

            // Make sure repository returns the correct information
            Assert.Equal(settingsExpected, _repository.Settings.First());

            // Clean up data
            _repository.Remove(settingsExpected);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveSettings: Complete");
        }

        [Fact]
        public void AddAndRemoveUserFromRoom()
        {
            // Create a new user and add it to the repository
            var user = new ChatUser()
            {
                Id = "4", 
                Name = "User 1",
                LastActivity = DateTime.Now
            };
            _repository.Add(user);

            // Create a new chat room and add it to the repository
            var creatorKey = _repository.Users.First().Key;
            var room = new ChatRoom()
            {
                Name = "Room 1",
                Closed = false,
                Topic = "Horses",
                Creator_Key = creatorKey
            };
            _repository.Add(room);

            // Add relationship between user and room
            _repository.AddUserRoom(user, room);

            // Verify the relationship was added properly
            Assert.True(user.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room));
            Assert.True(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(user));
            Assert.True(_repository.IsUserInRoom(user, room));

            // Remove the relationship
            _repository.RemoveUserRoom(user, room);

            // Verify the relationship was removed
            Assert.False(user.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(user));

            // Clean up data
            _repository.Remove(room);
            _repository.Remove(user);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveUserFromRoom: Complete");
        }

        [Fact]
        public void GetOnlineUsersByRoom()
        {
            // Create new users
            var user1 = new ChatUser()
            {
                Id = "5",
                Name = "User 1",
                LastActivity = DateTime.Now,
                Status = 0 // This evaluates to the UserStaus enum value "Active"
            };
            _repository.Add(user1);

            var user2 = new ChatUser()
            {
                Id = "6",
                Name = "User 2",
                LastActivity = DateTime.Now,
                Status = 2 // This evaluates to the UserStaus enum value "Offline"
            };
            _repository.Add(user2);

            // Create a new chat room
            var creatorKey = _repository.Users.First().Key;
            var room = new ChatRoom()
            {
                Name = "Room 1",
                Closed = false,
                Topic = "Horses",
                Creator_Key = creatorKey
            };
            _repository.Add(room);

            // Add the two users to the room
            _repository.AddUserRoom(user1, room);
            _repository.AddUserRoom(user2, room);

            // Try to get online users by room
            var onlineExpected = new List<ChatUser>() { user1 };
            Assert.Equal(onlineExpected, _repository.GetOnlineUsers(room).ToList());

            // Now, update user2 to be online and try to get all online users
            _repository.GetUserById("6").Status = 0;
            _repository.CommitChanges();
            onlineExpected.Add(user2);
            Assert.Equal(onlineExpected, _repository.GetOnlineUsers().ToList());

            // Clean up data
            _repository.RemoveUserRoom(user1, room);
            _repository.RemoveUserRoom(user2, room);
            _repository.Remove(room);
            _repository.Remove(user1);
            _repository.Remove(user2);

            Console.WriteLine("\tPersistedRepositoryTest.GetOnlineUsersByRoom: Complete");
        }

        [Fact]
        public void GetUserByName()
        {
            // Create new users
            var user1 = new ChatUser()
            {
                Id = "7",
                Name = "User 1",
                LastActivity = DateTime.Now,
            };
            _repository.Add(user1);

            var user2 = new ChatUser()
            {
                Id = "8",
                Name = "User 2",
                LastActivity = DateTime.Now,
            };
            _repository.Add(user2);

            var user3 = new ChatUser()
            {
                Id = "9",
                Name = "User 3",
                LastActivity = DateTime.Now,
            };
            _repository.Add(user3);

            // First, search users by partial name and verify it returns correctly
            var searchByU = new List<ChatUser>() { user1, user2, user3 };
            Assert.Equal(searchByU, _repository.SearchUsers("U"));

            // Now, verify getting a user by specific name
            Assert.Equal(user1, _repository.GetUserByName("User 1"));

            // Clean up data
            _repository.Remove(user1);
            _repository.Remove(user2);
            _repository.Remove(user3);

            Console.WriteLine("\tPersistedRepositoryTest.GetUserByName: Complete");
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

    }
}
