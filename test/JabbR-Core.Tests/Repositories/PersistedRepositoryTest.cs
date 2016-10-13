using Xunit;
using System;
using System.Linq;
using JabbR_Core.Data.Models;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace JabbR_Core.Tests.Repositories
{
    public class PersistedRepositoryTest
    {
        JabbrContext _context;
        PersistedRepository _repository;
        DbContextOptionsBuilder<JabbrContext> _options;

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

        // Test Rooms
        ChatRoom room1 = new ChatRoom()
        {
            Name = "Room 1",
            Closed = false,
            Topic = "Horses"
        };

        ChatRoom room2 = new ChatRoom()
        {
            Name = "Room 2",
            Closed = false,
            Topic = "Jenga"
        };


        // Test Client
        ChatClient client1 = new ChatClient()
        {
            Id = "1",
            LastActivity = DateTime.Now,
            LastClientActivity = DateTime.Now,
        };


        // Test Settings
        Settings settings1 = new Settings()
        {
            RawSettings = "These are my test settings."
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

        // Test Notifications
        Notification notification1 = new Notification()
        {
            Read = false
        };

        Notification notification2 = new Notification()
        {
            Read = false
        };


        // Test Identity
        ChatUserIdentity identity1 = new ChatUserIdentity()
        {
            ProviderName = "Provider",
            Identity = "Identity Name"
        };


        // Test ChatPrivateRoomUsers relationship objects
        ChatPrivateRoomUsers isAllowedR1 = new ChatPrivateRoomUsers();
        ChatPrivateRoomUsers isAllowedR2 = new ChatPrivateRoomUsers();

        // Test ChatRoomOwners relationship object
        ChatRoomOwners isOwnerR1 = new ChatRoomOwners();

        public PersistedRepositoryTest()
        {
            // Set up the db context
            _options = new DbContextOptionsBuilder<JabbrContext>();
            _options.UseInMemoryDatabase();

            var options = _options.Options;
            _context = new JabbrContext(options);

            // Set up the repository
            _repository = new PersistedRepository(_context);
        }

        public void ClearDatabase()
        {
            _context.Database.EnsureDeleted();

            // Does nothing for InMemory, this matters
            // for the behaviour of Persisted databases 
            _context.Database.EnsureCreated();
        }

        [Fact]
        public void AddAndRemoveUser()
        {
            // Fighting side effects
            ClearDatabase();
            // Try to add the user to the repository
            _repository.Add(user1);

            // Make sure repository returns the correct information
            Assert.Equal(user1, _repository.Users.First());

            // Remove user
            _repository.Remove(user1);

            // Ensure users list is now empty
            Assert.Empty(_repository.Users.ToList());

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveUser: Complete");
        }

        [Fact]
        public void AddAndRemoveRoom()
        {
            // Damn those side effects!
            ClearDatabase();

            // Add a user to repository to populate the Creator_Key attribute in ChatRoom
            _repository.Add(user1);

            // Set the creator key then try to add the room to the repository
            room1.CreatorKey = _repository.Users.First().Key;
            _repository.Add(room1);

            // Make sure repository returns the correct information
            Assert.Equal(room1, _repository.Rooms.First());
            Assert.Equal(room1, _repository.GetRoomByName("Room 1"));

            // Remove the room
            _repository.Remove(room1);

            //Ensure the room was removed
            Assert.Null(_repository.GetRoomByName("Room 1"));

            // Clean up data
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveRoom: Complete");
        }

        [Fact]
        public void AddAndRemoveClient()
        {
            ClearDatabase();

            // Add a user to repository to populate the UserKey attribute in ChatClient
            _repository.Add(user1);

            // Create a new client
            client1.UserKey= _repository.Users.First().Key;
            _repository.Add(client1);

            // Make sure repository returns the correct information
            Assert.Equal(client1, _repository.Clients.First());
            Assert.Equal(client1, _repository.GetClientById("1"));

            // Remove the client
            _repository.Remove(client1);

            //Ensure the client was removed
            Assert.Null(_repository.GetClientById("1"));

            // Clean up data
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveClient: Complete");
        }

        [Fact]
        public void AddAndRemoveSettings()
        {
            ClearDatabase();

            // Try to add the settings to the repository
            _repository.Add(settings1);

            // Make sure repository returns the correct information
            Assert.Equal(settings1, _repository.Settings.First());

            // Remove settings from the repository
            _repository.Remove(settings1);

            // Ensure the settings were removed
            Assert.Empty(_repository.Settings);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveSettings: Complete");
        }

        [Fact]
        public void AddAndRemoveTwoUsersFromOneRoom()
        {
            // Fighting side effects
            ClearDatabase();

            // Add a user to the repository
            _repository.Add(user1);
            _repository.Add(user2);

            // Set the creator key and add the chat room to the repository
            room1.CreatorKey = _repository.Users.First().Key;
            _repository.Add(room1);

            // Add relationship between user and room
            _repository.AddUserRoom(user1, room1);
            _repository.AddUserRoom(user2, room1);

            // Verify the relationships were added properly
            Assert.True(user1.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room1));
            Assert.True(room1.Users.Select(r => r.ChatUserKeyNavigation).Contains(user1));
            Assert.True(_repository.IsUserInRoom(user1, room1));

            Assert.True(user2.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room1));
            Assert.True(room1.Users.Select(r => r.ChatUserKeyNavigation).Contains(user2));
            Assert.True(_repository.IsUserInRoom(user2, room1));

            // Remove user2 from the room
            _repository.RemoveUserRoom(user2, room1);

            // Verify the relationship was removed and that user1 is still in the room
            Assert.False(user2.Rooms.Select(u => u.ChatRoomKeyNavigation).Contains(room1));
            Assert.False(room1.Users.Select(r => r.ChatUserKeyNavigation).Contains(user2));
            Assert.False(_repository.IsUserInRoom(user2, room1));
            Assert.True(_repository.IsUserInRoom(user1, room1));

            // Clean up data
            _repository.RemoveUserRoom(user1, room1);
            _repository.Remove(room1);
            _repository.Remove(user1);
            _repository.Remove(user2);

            Console.WriteLine("\tPersistedRepositoryTest.AddAndRemoveTwoUsersFromOneRoom: Complete");
        }

        [Fact]
        public void GetOnlineUsersByRoom()
        {
            // Fighting side effects
            ClearDatabase();

            // Set user status and add two users to the repository
            user1.Status = 0;  // This evaluates to the UserStaus enum value "Active"
            _repository.Add(user1);
            user2.Status = 2;  // This evaluates to the UserStaus enum value "Offline"
            _repository.Add(user2);

            // Set the creator key for the chat room and add it to the repository
            room1.CreatorKey = _repository.Users.First().Key;
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
            // Fighting side effects
            ClearDatabase();

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
            ClearDatabase();

            // Create a new repo with the context to fight side effects         
            _repository = new PersistedRepository(_context);

            // Add a user to the repository
            _repository.Add(user1);

            // Set up a new chat room and add it to the repository
            room1.CreatorKey = _repository.Users.First().Key;
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

            // Verify previous messages and all messages are returned properly
            Assert.Equal(new List<ChatMessage>() { message1 }, _repository.GetPreviousMessages("2"));
            Assert.Equal(new List<ChatMessage>() { message1, message2 }, _repository.GetMessagesByRoom(room1));

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
            ClearDatabase();

            // Add a user to the repository
            _repository.Add(user1);
            _repository.Add(user2);

            // Set up a new chat room and add it to the repository
            room1.CreatorKey = _repository.Users.First().Key;
            _repository.Add(room1);

            // Add relationship between user and room
            _repository.AddUserRoom(user1, room1);
            _repository.AddUserRoom(user2, room1);

            // Set up the message for user1 and add it to the repository
            message1.RoomKey = room1.Key;
            message1.RoomKeyNavigation = room1;
            message1.UserKey = user1.Key;
            message1.UserKeyNavigation = user1;
            _repository.Add(message1);

            // Add message1 to user1 and room1's lists
            _repository.GetUserByName("User 1").ChatMessages.Add(message1);
            _repository.GetRoomByName("Room 1").ChatMessages.Add(message1);
            _repository.CommitChanges();

            // Set up the message for user2 and add it to the repository
            message2.RoomKey = room1.Key;
            message2.RoomKeyNavigation = room1;
            message2.UserKey = user2.Key;
            message2.UserKeyNavigation = user2;
            _repository.Add(message2);

            // Add message2 to user2 and room2's lists
            _repository.GetUserByName("User 2").ChatMessages.Add(message2);
            _repository.GetRoomByName("Room 1").ChatMessages.Add(message2);
            _repository.CommitChanges();

            // Set up a new notification for user1 and add it to the repository
            notification1.MessageKey = message1.Key;
            notification1.RoomKey = room1.Key;
            notification1.UserKey = user1.Key;
            _repository.Add(notification1);

            // Set up a new notification for user2 and add it to the repository
            notification2.MessageKey = message2.Key;
            notification2.RoomKey = room1.Key;
            notification2.UserKey = user2.Key;
            _repository.Add(notification2);

            // Verify notifications were added properly
            Assert.Equal(new List<Notification>() { notification1 }, _repository.GetNotificationsByUser(user1).ToList());
            Assert.Equal(new List<Notification>() { notification2 }, _repository.GetNotificationsByUser(user2).ToList());

            // Clean up data
            _repository.Remove(notification1);
            _repository.Remove(message1);
            _repository.Remove(message2);
            _repository.RemoveUserRoom(user1, room1);
            _repository.RemoveUserRoom(user2, room1);
            _repository.Remove(room1);
            _repository.Remove(user1);
            _repository.Remove(user2);

            Console.WriteLine("\tPersistedRepositoryTest.GetNotificationByUser: Complete");
        }

        [Fact]
        public void GetUserByIdentity()
        {
            ClearDatabase();

            // Add a user to the repository
            user1.Identity = "Identity Name";
            _repository.Add(user1);

            // Setup a ChatUserIdentity and add it to the repository
            identity1.UserKey = user1.Key;
            identity1.UserKeyNavigation = user1;
            _repository.Add(identity1);

            // Verify identity was added properly
            Assert.Equal(user1, _repository.GetUserByIdentity("Provider", "Identity Name"));
            Assert.Equal(user1, _repository.GetUserByLegacyIdentity("Identity Name"));

            // Clean up data
            _repository.Remove(identity1);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.GetUserByIdentity: Complete");
        }

        [Fact]
        public void GetAllowedRoomsTwoUsersTwoRooms()
        {
            ClearDatabase();

            // Add a user to the repository
            _repository.Add(user1);
            _repository.Add(user2);
            var u1Key = _repository.GetUserByName("User 1").Key;
            var u2Key = _repository.GetUserByName("User 2").Key;

            // Set up the rooms's creator key and private attributes then add them to the repository
            room1.CreatorKey = u1Key;
            room1.Private = true;
            _repository.Add(room1);
        
            room2.CreatorKey = u1Key;
            room2.Private = true;
            _repository.Add(room2);

            // Set up the ChatPrivateRoomUsers objects that will represent rooms user1 is allowed in
            isAllowedR1.ChatRoomKey = _repository.GetRoomByName("Room 1").Key;
            isAllowedR1.ChatUserKey = u1Key;
            isAllowedR1.ChatRoomKeyNavigation = room1;
            isAllowedR1.ChatUserKeyNavigation = user1;

            isAllowedR2.ChatRoomKey = _repository.GetRoomByName("Room 2").Key;
            isAllowedR2.ChatUserKey = u2Key;
            isAllowedR2.ChatRoomKeyNavigation = room2;
            isAllowedR2.ChatUserKeyNavigation = user2;

            // Add the relationships to the rooms' allowed users lists and the user's allowed rooms list
            _repository.GetRoomByName("Room 1").AllowedUsers.Add(isAllowedR1);
            _repository.GetRoomByName("Room 2").AllowedUsers.Add(isAllowedR2);
            _repository.GetUserByName("User 1").AllowedRooms.Add(isAllowedR1);
            _repository.GetUserByName("User 2").AllowedRooms.Add(isAllowedR2);
            _repository.CommitChanges();
        
            // Verify GetAllowedRooms returns both rooms
            Assert.Equal(new List<ChatRoom>() { room1 }, _repository.GetAllowedRooms(user1).ToList());
        
            // Unallow user1 from room2
            _repository.GetUserByName("User 2").AllowedRooms.Remove(isAllowedR2);
            _repository.GetRoomByName("Room 2").AllowedUsers.Remove(isAllowedR2);
            _repository.Remove(isAllowedR2);
        
            // Verify GetAllowedRooms only returns one room
            Assert.Empty(_repository.GetAllowedRooms(user2).ToList());
        
            // Clean up data
            _repository.Remove(isAllowedR1);
            _repository.Remove(room1);
            _repository.Remove(room2);
            _repository.Remove(user1);
            _repository.Remove(user2);

            Console.WriteLine("\tPersistedRepositoryTest.GetAllowedRoomsTwoUsersTwoRooms: Complete");
        }

        [Fact]
        public void GetAllowedRoomsOneUserTwoRooms()
        {
            ClearDatabase();

            // Add a user to the repository
            _repository.Add(user1);
            var u1Key = _repository.Users.First().Key;

            // Set up the rooms's creator key and private attributes then add them to the repository
            room1.CreatorKey = u1Key;
            room1.Private = true;
            _repository.Add(room1);

            room2.CreatorKey = u1Key;
            room2.Private = true;
            _repository.Add(room2);

            // Create the ChatPrivateRoomUsers objects that will represent rooms user1 is allowed in
            isAllowedR1.ChatRoomKey = _repository.GetRoomByName("Room 1").Key;
            isAllowedR1.ChatUserKey = u1Key;
            isAllowedR1.ChatRoomKeyNavigation = room1;
            isAllowedR1.ChatUserKeyNavigation = user1;

            isAllowedR2.ChatRoomKey = _repository.GetRoomByName("Room 2").Key;
            isAllowedR2.ChatUserKey = u1Key;
            isAllowedR2.ChatRoomKeyNavigation = room2;
            isAllowedR2.ChatUserKeyNavigation = user1;

            // Add the relationships to the rooms' allowed users lists and the user's allowed rooms list
            _repository.GetRoomByName("Room 1").AllowedUsers.Add(isAllowedR1);
            _repository.GetRoomByName("Room 2").AllowedUsers.Add(isAllowedR2);
            _repository.Users.First().AllowedRooms.Add(isAllowedR1);
            _repository.Users.First().AllowedRooms.Add(isAllowedR2);
            _repository.CommitChanges();

            // Verify GetAllowedRooms returns both rooms
            Assert.Equal(new List<ChatRoom>() { room1, room2 }, _repository.GetAllowedRooms(user1).ToList());

            // Unallow user1 from room2
            _repository.Users.First().AllowedRooms.Remove(isAllowedR2);
            _repository.GetRoomByName("Room 2").AllowedUsers.Remove(isAllowedR2);
            _repository.Remove(isAllowedR2);

            // Verify GetAllowedRooms only returns one room
            Assert.Equal(new List<ChatRoom>() { room1 }, _repository.GetAllowedRooms(user1).ToList());

            // Clean up data
            _repository.Remove(isAllowedR1);
            _repository.Remove(room1);
            _repository.Remove(room2);
            _repository.Remove(user1);

            Console.WriteLine("\tPersistedRepositoryTest.GetAllowedRoomsOneUserTwoRooms: Complete");
        }

        [Fact]
        public void AddAndRemoveRoomOwner()
        {
            ClearDatabase();

            // Add a user to the repository
            _repository.Add(user1);
            var u1Key = _repository.Users.First().Key;

            // Set up the rooms's creator key and private attributes then add them to the repository
            room1.CreatorKey = u1Key;
            _repository.Add(room1);

            // Create the ChatRoomOwners object that will represent user1 being an owner
            isOwnerR1.ChatRoomKey = _repository.GetRoomByName("Room 1").Key;
            isOwnerR1.ChatUserKey = u1Key;
            isOwnerR1.ChatRoomKeyNavigation = room1;
            isOwnerR1.ChatUserKeyNavigation = user1;

            // Add the relationships to the rooms' allowed users lists and the user's allowed rooms list
            _repository.GetRoomByName("Room 1").Owners.Add(isOwnerR1);
            _repository.Users.First().OwnedRooms.Add(isOwnerR1);

            // Now that the data is set up, add the relationship to the repository 
            _repository.Add(isOwnerR1);

            // Verify the ownership relationship exists
            Assert.True(_context.ChatRoomOwners.ToList().Contains(isOwnerR1));
            Assert.True(_repository.Users.First().OwnedRooms.Contains(isOwnerR1));
            Assert.True(_repository.Rooms.First().Owners.Contains(isOwnerR1));

            // Remove the ownership relationship
            _repository.Users.First().OwnedRooms.Remove(isOwnerR1);
            _repository.Rooms.First().Owners.Remove(isOwnerR1);
            _repository.Remove(isOwnerR1);

            // Verify the ownership relationship doesn't exist
            Assert.False(_context.ChatRoomOwners.ToList().Contains(isOwnerR1));
            Assert.False(_repository.Users.First().OwnedRooms.Contains(isOwnerR1));
            Assert.False(_repository.Rooms.First().Owners.Contains(isOwnerR1));

            // Clean up data
            _repository.Remove(room1);
            _repository.Remove(user1);
        }

        [Fact]
        public void GetUserByRequestResetPasswordId()
        {
            ClearDatabase();

            // Set up the user and add them to the repository
            user1.RequestPasswordResetId = "12345";
            user1.RequestPasswordResetValidThrough = DateTimeOffset.Now.AddDays(1);
            _repository.Add(user1);

            // Add two more users with the wrong ids
            user2.RequestPasswordResetId = "11123";
            user2.RequestPasswordResetValidThrough = DateTimeOffset.Now.AddDays(3);
            _repository.Add(user2);

            user3.RequestPasswordResetId = "12332";
            user3.RequestPasswordResetValidThrough = DateTimeOffset.Now.AddDays(3);
            _repository.Add(user3);

            // Verify the correct user is returned
            Assert.Equal(user1, _repository.GetUserByRequestResetPasswordId("User 1", "12345"));

            // Clean up data
            _repository.Remove(user1);
            _repository.Remove(user2);
            _repository.Remove(user3);
        }

    }
}
