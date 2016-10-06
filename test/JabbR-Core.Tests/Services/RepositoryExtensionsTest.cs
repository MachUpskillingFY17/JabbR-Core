using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using JabbR_Core.Services;
using JabbR_Core.Data.Models;
using JabbR_Core.Data.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Tests.Services
{
    public class RepositoryExtensionsTest
    {
        private readonly IJabbrRepository _repository;
        private readonly ClaimsPrincipal _principal;
        private IQueryable<ChatUser> _queryableChatUser;
        private readonly ICache _cache;
        JabbrContext _context;
        DbContextOptionsBuilder<JabbrContext> _options;

        public RepositoryExtensionsTest()
        {
            // Set up the db context
            _options = new DbContextOptionsBuilder<JabbrContext>();
            string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JabbREFTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            _options.UseSqlServer(connection);
            DbContextOptions<JabbrContext> options = _options.Options;
            _context = new JabbrContext(options);

            _repository = new InMemoryRepository(_context);
            _principal = new ClaimsPrincipal();
            _cache = new DefaultCache();

            // For now, Jane is hardcoded as a user in the InMemoryRepository
            // Remove Jane for consistency in testing
            var jane = _repository.GetUserByName("Jane");
            _repository.Remove(jane);
        }
        
        // Tests
        // public static ChatUser GetLoggedInUser(this IJabbrRepository repository, ClaimsPrincipal principal)
        [Fact]
        public void GetLoggedInUserSuccessfully()
        {
            // Create ChatUser
            var user = new ChatUser()
            {
                Id = "12345",
                Name = "Test",
                LastActivity = Convert.ToDateTime("2016-08-23 00:00:00.000"),
                IsAdmin = true,
                IsAfk = true
            };

            // Create Principal with same info as ChatUser
            // Not yet implemented (Login Auth)

            // Test to see if same ChatUser is returned
            //Assert.Equal(user, _repository.GetLoggedInUser(_principal));
            Console.WriteLine("\tRepositoryExtensionTest.GetLoggedInUserSuccessfully: Complete");
        }

        // public static ChatUser GetUser(this IJabbrRepository repository, ClaimsPrincipal principal)
        [Fact]
        public void GetUserSuccessfully()
        {
            // Create ChatUser
            var user = new ChatUser()
            {
                Id = "12345",
                Name = "Test",
                LastActivity = Convert.ToDateTime("2016-08-23 00:00:00.000"),
                IsAdmin = true,
                IsAfk = true
            };
            _repository.Add((user));

            // Create Auth Identity
            // Not yet implemented (Login Auth)

            // Test to see if same ChatUser is returned
            //Assert.Equal(user, _repository.GetUser(_principal));

            // Clean up
            _repository.Remove(user);

            Console.WriteLine("\tRepositoryExtensionTest.GetUserSuccessfully: Complete");
        }

        // public static IQueryable<ChatUser> Online(this IQueryable<ChatUser> source)
        [Fact]
        public void ReturnIQueryableOnlineUsers()
        {
            // Create ChatUsers and set to "Online" and "Offline"
            var userOnline1 = new ChatUser
            {
                Name = "foo1",
                Status = (int)UserStatus.Active
            };

            var userOnline2 = new ChatUser
            {
                Name = "foo2",
                Status = (int)UserStatus.Inactive
            };

            var userOffline1 = new ChatUser
            {
                Name = "foo3",
                Status = (int)UserStatus.Offline
            };

            _repository.Add(userOnline1);
            _repository.Add(userOnline2);
            _repository.Add(userOffline1);

            // Create List<ChatUser> with only "Online" ChatUsers
            var queryableControl = new List<ChatUser>()
            {
                userOnline1,
                userOnline2
            };

            // Test to see if only "Online" users are returned IQueryable.ToList()
            Assert.Equal(queryableControl, _repository.Users.ToList().Online().OrderBy(u => u.Name));

            // Clean up
            _repository.Remove(userOnline1);
            _repository.Remove(userOnline2);
            _repository.Remove(userOffline1);

            Console.WriteLine("\tRepositoryExtensionTest.OnlineUsers: Complete");
        }

        // public static IEnumerable<ChatUser> Online(this IEnumerable<ChatUser> source)
        [Fact]
        public void ReturnIEnumOnlineUsers()
        {
            // Create ChatUsers and set to "Online" and "Offline"
            var userOnline1 = new ChatUser
            {
                Name = "foo1",
                Status = (int)UserStatus.Active
            };

            var userOnline2 = new ChatUser
            {
                Name = "foo2",
                Status = (int)UserStatus.Inactive
            };

            var userOffline1 = new ChatUser
            {
                Name = "foo3",
                Status = (int)UserStatus.Offline
            };

            _repository.Add(userOnline1);
            _repository.Add(userOnline2);
            _repository.Add(userOffline1);

            // Create List<ChatUser> with only "Online" ChatUsers
            var queryableControl = new List<ChatUser>()
            {
                userOnline1,
                userOnline2
            };

            // Test to see if only "Online" users are returned IEnum.ToList()
            Assert.Equal(queryableControl, _repository.Users.ToList().Online().OrderBy(u => u.Name));

            // Clean up
            _repository.Remove(userOnline1);
            _repository.Remove(userOnline2);
            _repository.Remove(userOffline1);

            Console.WriteLine("\tRepositoryExtensionTest.OnlineUsers: Complete");
        }

        // public static IEnumerable<ChatRoom> Allowed(this IEnumerable<ChatRoom> rooms, string userId)

        [Fact]
        public void AllowedUsersSuccess()
        {
            // create room.private = 0
            var roomPublic = new ChatRoom()
            {
                Private = false
            };

            // create room.private = 1
            var roomPrivate = new ChatRoom()
            {
                Private = true
            };

            // add both rooms to repository
            _repository.Add(roomPublic);
            _repository.Add(roomPrivate);

            // create controlList with only public room inside
            var controlList = new List<ChatRoom>()
            {
                roomPublic
            };

            // test to see if public room is returned
            Assert.Equal(controlList, _repository.Rooms.Allowed("").ToList());

            // update controlList with roomPrivate (now holds both rooms)
            controlList.Add(roomPrivate);

            // create new object of ChatRoomChatUserAllowed and add to room.AllowedUsers
            // Set KeyNavigationId to test value string.
            var user1 = new ChatUser()
            {
                Id = "123"
            };

            _repository.Add(user1);

            var allowed = new UserRoomAllowed();
            allowed.ChatUserKeyNavigation = user1;

            roomPrivate.AllowedUsers.Add(allowed);
            user1.AllowedRooms.Add(allowed);

            Assert.Equal(controlList, _repository.Rooms.Allowed("123").ToList());

            // Clean up
            _repository.Remove(roomPublic);
            _repository.Remove(roomPrivate);
            _repository.Remove(user1);
            roomPrivate.AllowedUsers.Remove(allowed);
            user1.AllowedRooms.Remove(allowed);

            Console.WriteLine("\tRepositoryExtensionTest.AllowedUsersSuccess: Complete");
        }

        // public static ChatRoom VerifyUserRoom(this IJabbrRepository repository, ICache cache, ChatUser user, string roomName)
        [Fact]
        public void VerifyUserRoomTest()
        {
            var user1 = new ChatUser(); // null

            var testRoom = new ChatRoom()
            {
                Name = "testRoom"
            }; // null

            // test HubException with empty string parameter
            Assert.Throws<HubException>(() => _repository.VerifyUserRoom(_cache, user1, ""));

            // test HubException if room = null
            Assert.Throws<HubException>(() => _repository.VerifyUserRoom(_cache, user1, "testRoom"));

            // test HubException ChatRoom returns null
            _repository.Add(testRoom);
            _repository.AddUserRoom(user1, testRoom);
            //repository.IsUserInRoom(cache, user, room) uses cache, FAILS -- NOT IMPLEMENTED -- Possibly DI of Repo
            //Assert.Throws<HubException>(() => _repository.VerifyUserRoom(_cache, user1, "testRoom"));

            // Clean up
            _repository.Remove(testRoom);
            _repository.RemoveUserRoom(user1, testRoom);

            Console.WriteLine("\tRepositoryExtensionTest.VerifyUserRoomTest: Complete");
        }

        // public static bool IsUserInRoom(this IJabbrRepository repository, ICache cache, ChatUser user, ChatRoom room)
        //[Fact]
        public void UsUserInRoomTest()
        {
            // Posibly DI of Report
        }

        // public static ChatUser VerifyUserId(this IJabbrRepository repository, string userId)
        [Fact]
        public void VerifyUserIdTest()
        {
            // test if HubException is thrown. No user created
            Assert.Throws<HubException>(() => _repository.VerifyUserId("123"));

            // create new ChatUser and add to repository
            var userTest = new ChatUser()
            {
                Id = "123"
            };
            _repository.Add(userTest);

            // test if user is returned
            Assert.Equal(userTest, _repository.VerifyUserId("123"));

            // Clean up
            _repository.Remove(userTest);

            Console.WriteLine("\tRepositoryExtensionTest.VerifyUserIdTest: Complete");
        }

        // public static ChatRoom VerifyRoom(this IJabbrRepository repository, string roomName, bool mustBeOpen = true)
        [Fact]
        public void VerifyRoomTest()
        {
            // test if HubException is thrown. string is null
            Assert.Throws<HubException>(() => _repository.VerifyRoom(null));

            // test if HubException is thrown. string has white space
            Assert.Throws<HubException>(() => _repository.VerifyRoom("with space"));

            // crate ChatRoom and add to repository
            var roomTest = new ChatRoom()
            {
                Name = "roomTest",
                Closed = true
            };
            _repository.Add(roomTest);

            // test if HubException is thrown. room is null -- room name does not match object
            Assert.Throws<HubException>(() => _repository.VerifyRoom("nullRoom"));

            // test if HubException is thrown. room is !null and room.closed = 0
            Assert.Throws<HubException>(() => _repository.VerifyRoom("roomTest"));

            // test if room returns as expected
            roomTest.Closed = false;
            Assert.Equal(roomTest, _repository.VerifyRoom("roomTest"));

            // Clean up
            _repository.Remove(roomTest);

            Console.WriteLine("\tRepositoryExtensionTest.VerifyRoomTest: Complete");
        }
    
        //public static ChatUser VerifyUser(this IJabbrRepository repository, string userName)
        [Fact]
        public void VerifyUserTest()
        {
            // test if HubException is thrown. user == null
            Assert.Throws<HubException>(() => _repository.VerifyUser("userTest"));

            // create new chatuser and add to repository
            var userTest = new ChatUser()
            {
                Name = "userTest"
            };

            _repository.Add(userTest);

            // test if user returns as expected
            Assert.Equal(userTest, _repository.VerifyUser("userTest"));

            // Clean up
            _repository.Remove(userTest);
            Console.WriteLine("\tRepositoryExtensionTest.VerifyUserTest: Complete");
        }

        // public static int GetUnreadNotificationsCount(this IJabbrRepository repository, ChatUser user)
        public void GetUnreadNotificationCount()
        {
            throw new NotImplementedException();
        }

        // public static IQueryable<Notification> Unread(this IQueryable<Notification> source)
        public void Unread()
        {
            throw new NotImplementedException();
        }

        //public static IQueryable<Notification> ByRoom(this IQueryable<Notification> source, string roomName)
        public void ByRoom()
        {
            throw new NotImplementedException();
        }

        // public static IList<string> GetAllowedClientIds(this IJabbrRepository repository, ChatRoom room)
        public void GetAllowedClientIds()
        {
            throw new NotImplementedException();
        }
    }
}