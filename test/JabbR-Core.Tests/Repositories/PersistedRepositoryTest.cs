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

        // Test Users
        ChatUser user1 = new ChatUser()
        {
            Id = "1",
            Name = "User 1",
            LastActivity = DateTime.Now,
        };

        ChatUser user2 = new ChatUser()
        {
            Id = "2",
            Name = "User 2",
            LastActivity = DateTime.Now,
        };

        ChatUser user3 = new ChatUser()
        {
            Id = "3",
            Name = "User 3",
            LastActivity = DateTime.Now,
        };

        // Test Room
        ChatRoom room1 = new ChatRoom()
        {
            Name = "Room 1",
            Closed = false,
            Topic = "Horses"
        };

        // Test Messages
        ChatMessage message1 = new ChatMessage()
        {
            Id = "1",
            When = DateTime.MinValue,
            MessageType = 1,
            HtmlEncoded = false
        };

        ChatMessage message2 = new ChatMessage()
        {
            Id = "2",
            When = DateTime.Now,
            MessageType = 1,
            HtmlEncoded = false
        };

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
            // Try to add the user to the repository
            _repository.Add(user1);

            // Make sure repository returns the correct information
            Assert.Equal(user1, _repository.Users.First());

            // Clean up data
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveUser: Complete");
        }

        [Fact]
        public void AddAndRemoveRoom()
        {
            // Add a user to repository to populate the Creator_Key attribute in ChatRoom
            _repository.Add(user1);

            // Set the creator key then try to add the room to the repository
            room1.Creator_Key = _repository.Users.First().Key;
            _repository.Add(room1);

            // Make sure repository returns the correct information
            Assert.Equal(room1, _repository.Rooms.First());
            Assert.Equal(room1, _repository.GetRoomByName("Room 1"));

            // Clean up data
            _repository.Remove(room1);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveRoom: Complete");
        }

        [Fact]
        public void AddAndRemoveClient()
        {
            // Add a user to repository to populate the UserKey attribute in ChatClient
            _repository.Add(user1);

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
            _repository.Remove(user1);

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
            // Add a user to the repository
            _repository.Add(user1);

            // Set the creator key and add the chat room to the repository
            room1.Creator_Key = _repository.Users.First().Key;
            _repository.Add(room1);

            // Add relationship between user and room
            _repository.AddUserRoom(user1, room1);

            // Verify the relationship was added properly
            Assert.True(user1.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room1));
            Assert.True(room1.Users.Select(r => r.ChatUserKeyNavigation).Contains(user1));
            Assert.True(_repository.IsUserInRoom(user1, room1));

            // Remove the relationship
            _repository.RemoveUserRoom(user1, room1);

            // Verify the relationship was removed
            Assert.False(user1.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room1));
            Assert.False(room1.Users.Select(r => r.ChatUserKeyNavigation).Contains(user1));
            Assert.False(_repository.IsUserInRoom(user1, room1));

            // Clean up data
            _repository.Remove(room1);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveUserFromRoom: Complete");
        }

        [Fact]
        public void GetOnlineUsersByRoom()
        {
            // Set user status and add two users to the repository
            user1.Status = 0;  // This evaluates to the UserStaus enum value "Active"
            _repository.Add(user1);
            user2.Status = 2;  // This evaluates to the UserStaus enum value "Offline"
            _repository.Add(user2);

            // Set the creator key for the chat room and add it to the repository
            room1.Creator_Key = _repository.Users.First().Key;
            _repository.Add(room1);

            // Add the two users to the room
            _repository.AddUserRoom(user1, room1);
            _repository.AddUserRoom(user2, room1);

            // Try to get online users by room
            var onlineExpected = new List<ChatUser>() { user1 };
            Assert.Equal(onlineExpected, _repository.GetOnlineUsers(room1).ToList());

            // Now, update user2 to be online and try to get all online users
            _repository.GetUserById("2").Status = 0;
            _repository.CommitChanges();
            onlineExpected.Add(user2);
            Assert.Equal(onlineExpected, _repository.GetOnlineUsers().ToList());

            // Clean up data
            _repository.RemoveUserRoom(user1, room1);
            _repository.RemoveUserRoom(user2, room1);
            _repository.Remove(room1);
            _repository.Remove(user1);
            _repository.Remove(user2);

            Console.WriteLine("\tPersistedRepositoryTest.GetOnlineUsersByRoom: Complete");
        }

        [Fact]
        public void GetUserByName()
        {
            // Add three users to the repo
            _repository.Add(user1);
            _repository.Add(user2);
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

        [Fact]
        public void GetMessagesByRoomAndId()
        {
            // Add a user to the repository
            _repository.Add(user1);

            // Set up a new chat room and add it to the repository
            room1.Creator_Key = _repository.Users.First().Key;
            _repository.Add(room1);

            // Add relationship between user and room
            _repository.AddUserRoom(user1, room1);

            // Set up the message and add it to the repository
            message1.RoomKey = room1.Key;
            message1.RoomKeyNavigation = room1;
            message1.UserKey = user1.Key;
            message1.UserKeyNavigation = user1;
            _repository.Add(message1);

            // Add message to user and room's lists
            _repository.GetUserByName("User 1").ChatMessages.Add(message1);
            _repository.GetRoomByName("Room 1").ChatMessages.Add(message1);
            _repository.CommitChanges();

            // Verify message was added properly
            Assert.Equal(new List<ChatMessage>() { message1 }, _repository.GetMessagesByRoom(room1));
            Assert.Equal(message1, _repository.GetMessageById("1"));

            // Set up and add another message
            message2.RoomKey = room1.Key;
            message2.RoomKeyNavigation = room1;
            message2.UserKey = user1.Key;
            message2.UserKeyNavigation = user1;
            _repository.Add(message2);

            // Add message to user and room's lists
            _repository.GetUserByName("User 1").ChatMessages.Add(message1);
            _repository.GetRoomByName("Room 1").ChatMessages.Add(message1);
            _repository.CommitChanges();

            // Verify previous messages are returned properly
            Assert.Equal(new List<ChatMessage>() { message1 }, _repository.GetPreviousMessages("2"));

            // Clean up data
            _repository.Remove(message1);
            _repository.Remove(message2);
            _repository.RemoveUserRoom(user1, room1);
            _repository.Remove(room1);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.GetMessagesByRoomAndId: Complete");
        }

        [Fact]
        public void GetNotificationsByUser()
        {
            // Add a user to the repository
            _repository.Add(user1);

            // Set up a new chat room and add it to the repository
            room1.Creator_Key = _repository.Users.First().Key;
            _repository.Add(room1);

            // Add relationship between user and room
            _repository.AddUserRoom(user1, room1);

            // Set up the message and add it to the repository
            message1.RoomKey = room1.Key;
            message1.RoomKeyNavigation = room1;
            message1.UserKey = user1.Key;
            message1.UserKeyNavigation = user1;
            _repository.Add(message1);

            // Add message to user and room's lists
            _repository.GetUserByName("User 1").ChatMessages.Add(message1);
            _repository.GetRoomByName("Room 1").ChatMessages.Add(message1);
            _repository.CommitChanges();

            // Create a new notification and add it to the repository
            var notification = new Notification()
            {
                MessageKey = message1.Key,
                RoomKey = room1.Key,
                UserKey = user1.Key,
                Read = false
            };
            _repository.Add(notification);

            // Verify notification was added properly
            Assert.Equal(new List<Notification>() { notification }, _repository.GetNotificationsByUser(user1).ToList());

            // Clean up data
            _repository.Remove(notification);
            _repository.Remove(message1);
            _repository.RemoveUserRoom(user1, room1);
            _repository.Remove(room1);
            _repository.Remove(user1);
        }

        public IQueryable<ChatRoom> GetAllowedRooms(ChatUser user)
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

    }
}
