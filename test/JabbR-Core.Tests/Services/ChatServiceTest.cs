using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Services;
using JabbR_Core.Data.Models;
using JabbR_Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Immutable;

namespace JabbR_Core.Tests.Services
{
    public class ChatServiceTest
    {
        ChatService chatService;
        IJabbrRepository _repository;
        ICache _cache;
        IRecentMessageCache _recentMessageCache;
        MemoryCache _memCache;
        IOptions<MemoryCacheOptions> optionsAccessor;
        JabbrContext _context;
        DbContextOptionsBuilder<JabbrContext> _options;

        public ChatServiceTest()
        {
            // Set up the db context and repository
            _options = new DbContextOptionsBuilder<JabbrContext>();
            string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JabbRChatTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            _options.UseInMemoryDatabase<JabbrContext>(connection);
            DbContextOptions<JabbrContext> options = _options.Options;
            _context = new JabbrContext(options);
            _repository = new Repository(_context);
            _recentMessageCache = new RecentMessageCache();

            // Delete the database and recreate a clean one to prepare for the next test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var setups = new List<IConfigureOptions<MemoryCacheOptions>>();
            //var action = new Action<ApplicationSettings>(s => s = _settings);
            //setups.Add(new ConfigureOptions<ApplicationSettings>(action));
            optionsAccessor = new OptionsManager<MemoryCacheOptions>(setups);
            _memCache = new MemoryCache(optionsAccessor);
            _cache = new DefaultCache(_memCache);

            chatService = new ChatService(_cache, _recentMessageCache, _repository);
        }


        //TODO: write tests for each of these functions. 
        //Addroom tests
        [Fact]
        public void ThrowsIfRoomNameIsLobby()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            Assert.Throws<HubException>(() => chatService.AddRoom(user, "Lobby"));
            Assert.Throws<HubException>(() => chatService.AddRoom(user, "LObbY"));
        }

        [Fact]
        public void ThrowsIfRoomNameInvalid()
        {
            var user = new ChatUser
            {
                Name = "foo",
                Id = "1",
                LastActivity = DateTime.Now
            };
            _repository.Add(user);

            Assert.Throws<HubException>(() => chatService.AddRoom(user, "Invalid name"));
        }

        [Fact]
        public void ThrowsIfRoomCreationDisabledAndNotAdmin()
        {
            var user = new ChatUser
            {
                Name = "foo",
                Id = "2",
                LastActivity = DateTime.Now
            };
            _repository.Add(user);
            var settings = new ApplicationSettings
            {
                AllowRoomCreation = false
            };

            var falseChatService = new ChatService(_cache, _recentMessageCache, _repository, settings);

            Assert.Throws<HubException>(() => falseChatService.AddRoom(user, "NewRoom"));
        }

        [Fact]
        public void DoesNotThrowIfRoomCreationDisabledAndUserAdmin()
        {
            var user = new ChatUser
            {
                Name = "foo",
                Id = "3",
                LastActivity = DateTime.Now,
                IsAdmin = true
            };
            _repository.Add(user);
            var settings = new ApplicationSettings
            {
                AllowRoomCreation = false
            };
            var falseChatService = new ChatService(_cache, _recentMessageCache, _repository, settings);

            ChatRoom room = falseChatService.AddRoom(user, "NewRoom");

            Assert.NotNull(room);
            Assert.Equal("NewRoom", room.Name);
            Assert.Same(room, _repository.GetRoomByName("NewRoom"));
            Assert.True(room.Owners.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
            Assert.Same(room.CreatorKeyNavigation, user);
            Assert.True(user.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        [Fact]
        public void ThrowsIfRoomNameContainsPeriod()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            Assert.Throws<HubException>(() => chatService.AddRoom(user, "Invalid.name"));
        }

        [Fact]
        public void AddsUserAsCreatorAndOwner()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            ChatRoom room = chatService.AddRoom(user, "NewRoom");

            Assert.NotNull(room);
            Assert.Equal("NewRoom", room.Name);
            Assert.Same(room, _repository.GetRoomByName("NewRoom"));
            Assert.True(room.Owners.Select(c => c.ChatUserKeyNavigation).Contains(user));
            Assert.Same(room.CreatorKeyNavigation, user);
            Assert.True(user.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        //JoinRoom tests
        [Fact]
        public void AddsUserToRoom()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            chatService.JoinRoom(user, room, null);

            Assert.True(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void AddsUserToRoomIfAllowedAndRoomIsPrivate()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            ChatPrivateRoomUsers cr = new ChatPrivateRoomUsers()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatUserKeyNavigation = user,
                ChatRoomKeyNavigation = room
            };
            room.AllowedUsers.Add(cr);
            user.AllowedRooms.Add(cr);
            _repository.Add(cr);

            chatService.JoinRoom(user, room, null);

            Assert.True(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void ThrowsIfRoomIsPrivateAndNotAllowed()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            Assert.Throws<HubException>(() => chatService.JoinRoom(user, room, null));
        }

        [Fact]
        public void AddsUserToRoomIfUserIsAdminAndRoomIsPrivate()
        {
            var user = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            chatService.JoinRoom(user, room, null);

            Assert.True(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void CanUpdateActivity()
        {
            var user = new ChatUser
            {
                Name = "foo",
                Status = (int)UserStatus.Inactive,
                IsAfk = true,
                AfkNote = "note!?"
            };
            _repository.Add(user);

            chatService.UpdateActivity(user, "client1", userAgent: null);
            var clients = user.ConnectedClients.ToList();

            Assert.Equal((int)UserStatus.Active, user.Status);
            Assert.Equal(1, clients.Count);
            Assert.Equal("client1", clients[0].Id);
            Assert.Same(user, clients[0].UserKeyNavigation);
            Assert.Null(user.AfkNote);
            Assert.False(user.IsAfk);
        }

        //LeaveRoom tests
        [Fact]
        public void RemovesUserFromRoom()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            _repository.AddUserRoom(user, room);

            chatService.LeaveRoom(user, room);

            Assert.False(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        //AddMessage tests
        [Fact]
        public void AddsNewMessageToRepository()
        {
            //create user and room
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            //create user/room relationship
            _repository.AddUserRoom(user, room);

            ChatMessage message = chatService.AddMessage(user, room, Guid.NewGuid().ToString(), "Content");

            Assert.NotNull(message);
            Assert.Same(message, room.ChatMessages.First());
            Assert.Equal("Content", message.Content);
        }


        //AddOwner tests
        [Fact]
        public void ThrowsIfUserIsNotOwner()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.AddOwner(user, user, room));
        }

        [Fact]
        public void ThrowsIfUserIsAlreadyAnOwner()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            _repository.AddUserRoom(user, room);

            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);

            Assert.Throws<HubException>(() => chatService.AddOwner(user, user, room));
        }

        [Fact]
        public void MakesUserOwner()
        {
            // User authorizing the new owner, this user is already a room owner
            var oldOwner = new ChatUser
            {
                Name = "foo",
                Id = "1"
            };
            _repository.Add(oldOwner);

            // This user will be made an owner of the room
            var newOwner = new ChatUser
            {
                Name = "foo2",
            };
            _repository.Add(newOwner);

            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = oldOwner,
                CreatorId = oldOwner.Id
            };
            _repository.Add(room);

            // Now that both the original owner and room have been created, add the owner relationship
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = oldOwner.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            room.Owners.Add(cro);
            oldOwner.OwnedRooms.Add(cro);
            _repository.Add(cro);

            // Try to add a new owner
            chatService.AddOwner(oldOwner, newOwner, room);

            // Verify owner was added
            Assert.True(room.Owners.Select(c => c.ChatUserKeyNavigation).ToList().Contains(newOwner));
            Assert.True(newOwner.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        [Fact]
        public void MakesUserOwnerIfUserAlreadyAllowed()
        {
            var oldOwner = new ChatUser
            {
                Name = "foo"
            };
            var newOwner = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(oldOwner);
            _repository.Add(newOwner);
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true,
                CreatorKeyNavigation = oldOwner
            };
            _repository.Add(room);

            // Now that both the original owner and room have been created, add the owner relationship
            _repository.AddUserRoom(oldOwner, room);
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = oldOwner.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            room.Owners.Add(cro);
            oldOwner.OwnedRooms.Add(cro);
            _repository.Add(cro);

            //Allow new owner into room
            ChatPrivateRoomUsers userAllowed = new ChatPrivateRoomUsers()
            {
                ChatRoomKey = room.Key,
                ChatUserId = newOwner.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = newOwner
            };
            newOwner.AllowedRooms.Add(userAllowed);
            room.AllowedUsers.Add(userAllowed);
            _repository.Add(userAllowed);

            chatService.AddOwner(oldOwner, newOwner, room);

            Assert.True(room.Owners.Select(c => c.ChatUserKeyNavigation).ToList().Contains(newOwner));
            Assert.True(newOwner.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        [Fact]
        public void MakesOwnerAllowedIfRoomLocked()
        {
            var oldOwner = new ChatUser
            {
                Name = "foo"
            };
            //Allowed user in room
            var allowedUsr = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(oldOwner);
            _repository.Add(allowedUsr);

            var room = new ChatRoom
            {
                Name = "Room",
                Private = true,
                CreatorKeyNavigation = oldOwner
            };
            _repository.Add(room);

            // Now that both the original owner and room have been created, add the owner relationship
            _repository.AddUserRoom(oldOwner, room);
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = oldOwner.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            oldOwner.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            _repository.Add(cro);

            chatService.AddOwner(oldOwner, allowedUsr, room);

            Assert.True(allowedUsr.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(allowedUsr));
            Assert.True(room.Owners.Select(c => c.ChatUserKeyNavigation).ToList().Contains(allowedUsr));
            Assert.True(allowedUsr.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        [Fact]
        public void NonOwnerAdminCanAddUserAsOwner()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(user);

            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = admin
            };
            _repository.Add(room);

            //create user/room relationship 
            _repository.AddUserRoom(admin, room);

            chatService.AddOwner(admin, user, room);

            Assert.True(room.Owners.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
            Assert.True(user.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        //RemoveOwner tests
        [Fact]
        public void ThrowsIfTargettedUserIsNotOwner()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var targetUser = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(targetUser);

            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user
            };
            _repository.Add(room);

            //Add both users to room
            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(targetUser, room);

            Assert.Throws<HubException>(() => chatService.RemoveOwner(user, targetUser, room));
        }

        [Fact]
        public void ThrowsIfActingUserIsNotCreatorOrAdmin()
        {
            var unspecialUser = new ChatUser
            {
                Name = "foo"
            };
            var user = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(unspecialUser);
            _repository.Add(user);

            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            //add unspecialUser and user relationships to room
            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(unspecialUser, room);

            //Make both owners
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = unspecialUser.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = unspecialUser
            };
            ChatRoomOwners cro2 = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            unspecialUser.OwnedRooms.Add(cro);
            room.Owners.Add(cro2);
            user.OwnedRooms.Add(cro2);
            _repository.Add(cro);
            _repository.Add(cro2);

            Assert.Throws<HubException>(() => chatService.RemoveOwner(unspecialUser, user, room));
        }

        [Fact]
        public void RemovesOwnerIfActingUserIsAdmin()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(user);

            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            //Create owner
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);

            //Create user/room relationship 
            _repository.AddUserRoom(admin, room);
            _repository.AddUserRoom(user, room);

            chatService.RemoveOwner(admin, user, room);

            Assert.False(room.Owners.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
            Assert.False(user.OwnedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }


        //KickUsers 
        [Fact]
        public void ThrowsIfKickSelf()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user
            };
            _repository.Add(room);

            //Add owner and user/room relationships
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.KickUser(user, user, room));;
        }

        [Fact]
        public void ThrowsIfUserIsNotOwnerAndNotAdmin()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var targetUser = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(targetUser);

            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(targetUser, room);

            Assert.Throws<HubException>(() => chatService.KickUser(user, targetUser, room));
        }

        [Fact]
        public void ThrowsIfTargetUserNotInRoom()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var targetUser = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(targetUser);

            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user
            };
            _repository.Add(room);

            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            _repository.Add(cro);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.KickUser(user, targetUser, room));
        }

        [Fact]
        public void ThrowsIfOwnerTriesToRemoveOwner()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var targetUser = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(targetUser);

            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            //Add user/room and owner relationships
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            ChatRoomOwners cro2 = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = targetUser.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };
            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            targetUser.OwnedRooms.Add(cro2);
            room.Owners.Add(cro2);

            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(targetUser, room);

            Assert.Throws<HubException>(() => chatService.KickUser(user, targetUser, room));
        }

        [Fact]
        public void DoesNotThrowIfCreatorKicksOwner()
        {
            // Create two users
            var user = new ChatUser
            {
                Name = "foo"
            };
            var targetUser = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(targetUser);

            // Create a room
            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user
            };
            _repository.Add(room);

            // Add both users as room owners
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            ChatRoomOwners cro2 = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = targetUser.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };
            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            targetUser.OwnedRooms.Add(cro2);
            room.Owners.Add(cro2);
            _repository.Add(cro);
            _repository.Add(cro2);

            // Add both users to the room
            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(targetUser, room);

            chatService.KickUser(user, targetUser, room);

            Assert.False(targetUser.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(targetUser));
        }

        [Fact]
        public void AdminCanKickUser()
        {
            // Create two users
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(user);

            // Create a room
            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            //add user/room relationships 
            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(admin, room);

            chatService.KickUser(admin, user, room);

            Assert.False(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void DoesNotThrowIfAdminKicksOwner()
        {
            // Create two users
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(user);

            // Add a room
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            // Add the room owner
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            _repository.Add(cro);

            //Add relationships
            _repository.AddUserRoom(user, room);
            _repository.AddUserRoom(admin, room);

            chatService.KickUser(admin, user, room);

            Assert.False(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void AdminCanKickCreator()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var creator = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(creator);

            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = creator
            };
            _repository.Add(room);

            // Set up the room owner
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = creator.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };
            creator.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            _repository.Add(cro);

            // Add the users to the room
            _repository.AddUserRoom(creator, room);
            _repository.AddUserRoom(admin, room);

            chatService.KickUser(admin, creator, room);

            Assert.False(creator.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(creator));
        }

        [Fact]
        public void ThrowsIfOwnerTriesToRemoveAdmin()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var owner = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(owner);

            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            //create relationships
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = owner.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };
            owner.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            _repository.Add(cro);

            _repository.AddUserRoom(admin, room);
            _repository.AddUserRoom(owner, room);

            Assert.Throws<HubException>(() => chatService.KickUser(owner, admin, room));
        }

        [Fact]
        public void AdminCanKickAdmin()
        {
            // Add two admins
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var otherAdmin = new ChatUser
            {
                Name = "foo2",
                IsAdmin = true
            };
            _repository.Add(admin);
            _repository.Add(otherAdmin);

            // Create a room
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            // Add both admins to the room
            _repository.AddUserRoom(admin, room);
            _repository.AddUserRoom(otherAdmin, room);

            chatService.KickUser(admin, otherAdmin, room);

            Assert.False(otherAdmin.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(otherAdmin));
        }

        //DisconnectClient tests
        [Fact]
        public void RemovesClientFromUserClientList()
        {
            var user = new ChatUser
            {
                Name = "foo",
                Status = (int)UserStatus.Inactive
            };
            user.ConnectedClients.Add(new ChatClient
            {
                Id = "foo",
                UserKeyNavigation = user
            });
            user.ConnectedClients.Add(new ChatClient
            {
                Id = "bar",
                UserKeyNavigation = user
            });

            _repository.Add(user);

            chatService.DisconnectClient("foo");

            Assert.Equal(1, user.ConnectedClients.Count);
            Assert.Equal("bar", user.ConnectedClients.First().Id);
        }

        [Fact]
        public void MarksUserAsOfflineIfNoMoreClients()
        {
            var user = new ChatUser
            {
                Id = "userId",
                Name = "foo",
                Status = (int)UserStatus.Inactive
            };
            user.ConnectedClients.Add(new ChatClient
            {
                Id = "foo",
                UserKeyNavigation = user
            });

            _repository.Add(user);

            string userId = chatService.DisconnectClient("foo");

            Assert.Equal(0, user.ConnectedClients.Count);
            Assert.Equal("userId", userId);
            Assert.Equal((int)UserStatus.Offline, user.Status);
        }

        //LockRoom tests

        [Fact]
        public void LocksRoomIfOwner()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            //add relationships 
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);

            _repository.AddUserRoom(user, room);

            chatService.LockRoom(user, room);

            Assert.True(room.Private);
            Assert.True(user.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void ThrowsIfRoomAlreadyLocked()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);
            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user,
                Private = true
            };
            _repository.Add(room);

            //Add owner, make the user allowed and add user to room
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);
            ChatPrivateRoomUsers cra = new ChatPrivateRoomUsers()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };           
            room.AllowedUsers.Add(cra);
            user.AllowedRooms.Add(cra);
            _repository.Add(cra);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.LockRoom(user, room));
        }

        [Fact]
        public void LocksRoom()
        {
            // Create a user
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            // Create a room
            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user
            };
            _repository.Add(room);

            // Make the user a room owner
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);

            // Add the user to the room
            _repository.AddUserRoom(user, room);

            chatService.LockRoom(user, room);

            Assert.True(room.Private);
            Assert.True(user.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));
        }

        [Fact]
        public void MakesAllUsersAllowed()
        {
            var creator = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(creator);

            // Create some online and offline users
            var users = Enumerable.Range(0, 5).Select(i => new ChatUser
            {
                Name = "user_" + i
            }).ToList();
            var offlineUsers = Enumerable.Range(6, 10).Select(i => new ChatUser
            {
                Name = "user_" + i,
                Status = (int)UserStatus.Offline
            }).ToList();

            // Create a room
            var room = new ChatRoom
            {
                Name = "room",
                CreatorKeyNavigation = creator
            };
            _repository.Add(room);

            // Add users to the repository and to the room
            foreach (var user in users)
            {
                _repository.Add(user);
                _repository.AddUserRoom(user, room);
            }
            foreach (var user in offlineUsers)
            {
                _repository.Add(user);
                _repository.AddUserRoom(user, room);
            }

            //Relationship
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = creator.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };
            room.Owners.Add(cro);
            creator.OwnedRooms.Add(cro);
            _repository.Add(cro);

            chatService.LockRoom(creator, room);

            foreach (var u in users)
            {
                Assert.True(u.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
                Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(u));
            }

            foreach (var u in offlineUsers)
            {
                Assert.False(u.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
                Assert.False(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(u));
            }
        }

        [Fact]
        public void LocksRoomIfAdmin()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            _repository.Add(admin);

            var room = new ChatRoom
            {
                Name = "Room"
            };
            _repository.Add(room);

            _repository.AddUserRoom(admin, room);

            chatService.LockRoom(admin, room);

            Assert.True(room.Private);
            Assert.True(admin.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(admin));
        }

        // Allow User tests
        [Fact]
        public void ThrowsIfRoomNotPrivate()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(user2);

            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.AllowUser(user, user2, room));
        }

        [Fact]
        public void ThrowsIfUserIsNotOwnerAllowUserVersion()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.AllowUser(user, user, room));
        }

        [Fact]
        public void ThrowsIfUserIsAlreadyAllowed()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(user2);

            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            //Add relationships
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);
            ChatPrivateRoomUsers cra = new ChatPrivateRoomUsers()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user2.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user2
            };
            room.AllowedUsers.Add(cra);
            user2.AllowedRooms.Add(cra);
            _repository.Add(cra);

            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.AllowUser(user, user2, room));
        }

        [Fact]
        public void AllowsUserIntoRoom()
        {
            var user = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user);
            _repository.Add(user2);

            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            //Add relationships
            ChatRoomOwners cro = new ChatRoomOwners()
            {
                ChatRoomKey = room.Key,
                ChatUserId = user.Id,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            _repository.Add(cro);

            _repository.AddUserRoom(user, room);

            chatService.AllowUser(user, user2, room);

            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user2));
            Assert.True(user2.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        [Fact]
        public void AdminCanAllowUserIntoRoom()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            _repository.Add(admin);
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            //add relationship
            _repository.AddUserRoom(admin, room);

            chatService.AllowUser(admin, admin, room);

            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(admin));
            Assert.True(admin.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
        }

        // Unallow user tests
        [Fact]
        public void ThrowsIfRoomNotPrivateUnallow()
        {
            // Create two users and add them to the repository
            var user1 = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user1);
            _repository.Add(user2);

            // Create a chat room
            var room = new ChatRoom
            {
                Name = "Room",
            };
            _repository.Add(room);

            // Make user1 the owner of room
            ChatRoomOwners uro = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user1,
            };
            room.Owners.Add(uro);
            user1.OwnedRooms.Add(uro);
            _repository.Add(uro);

            // Add user1 to the room and create the relationship between them
            _repository.AddUserRoom(user1, room);

            Assert.Throws<HubException>(() => chatService.UnallowUser(user1, user2, room));
        }

        [Fact]
        public void ThrowsIfTargetUserIsCreator()
        {
            // Create a new user and add it to the repository
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(user);

            // Create a new room and add it to the repository
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true,
                CreatorKeyNavigation = user
            };
            _repository.Add(room);

            // Make the user the owner of room
            ChatRoomOwners uro = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user,
            };
            room.Owners.Add(uro);
            user.OwnedRooms.Add(uro);
            _repository.Add(uro);

            // Allow the user in the room
            ChatPrivateRoomUsers ura = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user,
            };
            room.AllowedUsers.Add(ura);
            user.AllowedRooms.Add(ura);
            _repository.Add(ura);

            // Add the user to the room and create the relationship between them
            _repository.AddUserRoom(user, room);

            Assert.Throws<HubException>(() => chatService.UnallowUser(user, user, room));
        }

        [Fact]
        public void ThrowsIfUnallowingUserIsNotOwner()
        {
            // Create two users and add them to the repository
            var user1 = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user1);
            _repository.Add(user2);

            // Create a chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Add user1 to the room
            _repository.AddUserRoom(user1, room);

            Assert.Throws<HubException>(() => chatService.UnallowUser(user1, user2, room));
        }

        [Fact]
        public void DoesNotThrowIfUserIsAdminButIsNotOwner()
        {
            // Create two chat users and make one an admin
            var admin = new ChatUser
            {
                Id = "1",
                Name = "foo",
                IsAdmin = true
            };
            var user2 = new ChatUser
            {
                Id = "2",
                Name = "foo2"
            };
            _repository.Add(admin);
            _repository.Add(user2);

            // Create a new chat room
            var room = new ChatRoom
            {
                Key = 1,
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Add admin to the room
            _repository.AddUserRoom(admin, room);

            // Make user2 allowed in the room
            ChatPrivateRoomUsers ura = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user2
            };
            room.AllowedUsers.Add(ura);
            user2.AllowedRooms.Add(ura);
            _repository.Add(ura);

            // Have the admin unallow user2 from the room
            chatService.UnallowUser(admin, user2, room);

            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(user2));
            Assert.False(user2.Rooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).Contains(user2));
            Assert.False(user2.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
        }

        [Fact]
        public void ThrowsIfUserIsNotAllowed()
        {
            // Create two new users and add them to the repository
            var user1 = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user1);
            _repository.Add(user2);

            // Create a new room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Make user1 the owner of the room
            ChatRoomOwners uro = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user1
            };
            user1.OwnedRooms.Add(uro);
            room.Owners.Add(uro);
            _repository.Add(uro);

            // Add the user1 to the room
            _repository.AddUserRoom(user1, room);

            Assert.Throws<HubException>(() => chatService.UnallowUser(user1, user2, room));
        }

        [Fact]
        public void ThrowIfOwnerTriesToUnallowOwner()
        {
            // Create two users
            var user1 = new ChatUser()
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user1);
            _repository.Add(user2);

            // Create a new chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Make user1 and user2 owners of the room
            ChatRoomOwners uro1 = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user1
            };
            user1.OwnedRooms.Add(uro1);
            room.Owners.Add(uro1);
            _repository.Add(uro1);
            ChatRoomOwners uro2 = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user2
            };
            user2.OwnedRooms.Add(uro2);
            room.Owners.Add(uro2);
            _repository.Add(uro2);

            // Make user1 and user2 allowed in the room
            ChatPrivateRoomUsers ura1 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user1
            };
            user1.AllowedRooms.Add(ura1);
            room.AllowedUsers.Add(ura1);
            _repository.Add(ura1);
            ChatPrivateRoomUsers ura2 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user2
            };
            user2.AllowedRooms.Add(ura2);
            room.AllowedUsers.Add(ura2);
            _repository.Add(ura2);

            // Add user1 and user2 to the room
            _repository.AddUserRoom(user1, room);
            _repository.AddUserRoom(user2, room);

            Assert.Throws<HubException>(() => chatService.UnallowUser(user1, user2, room));
        }

        [Fact]
        public void UnallowsAndRemovesUserFromRoom()
        {
            // Create two users and add them to the repository
            var user1 = new ChatUser
            {
                Name = "foo"
            };
            var user2 = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(user1);
            _repository.Add(user2);

            // Create a chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Make user1 the owner of the room
            ChatRoomOwners uro = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user1
            };
            room.Owners.Add(uro);
            user1.OwnedRooms.Add(uro);
            _repository.Add(uro);

            // Add user1 to the room
            _repository.AddUserRoom(user1, room);

            // Make user2 allowed in the room
            ChatPrivateRoomUsers ura = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user2
            };
            room.AllowedUsers.Add(ura);
            user2.AllowedRooms.Add(ura);
            _repository.Add(ura);

            chatService.UnallowUser(user1, user2, room);

            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(user2));
            Assert.False(user2.Rooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).Contains(user2));
            Assert.False(user2.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
        }

        [Fact]
        public void AdminCanUnallowUser()
        {
            // Create two users and add them to the repository
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(admin);
            _repository.Add(user);

            // Create a chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Add the admin to the room
            _repository.AddUserRoom(admin, room);

            // Make both users allowed in the room
            ChatPrivateRoomUsers ura1 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.AllowedUsers.Add(ura1);
            admin.AllowedRooms.Add(ura1);
            _repository.Add(ura1);
            ChatPrivateRoomUsers ura2 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.AllowedUsers.Add(ura2);
            user.AllowedRooms.Add(ura2);
            _repository.Add(ura2);

            chatService.UnallowUser(admin, user, room);

            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(user));
            Assert.False(user.Rooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).Contains(user));
            Assert.False(user.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
        }

        [Fact]
        public void AdminCanUnallowOwner()
        {
            // Create two new users and add them to the repository
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var owner = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(admin);
            _repository.Add(owner);

            // Create a new chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Make owner the room owner
            ChatRoomOwners uro = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };
            room.Owners.Add(uro);
            owner.OwnedRooms.Add(uro);
            _repository.Add(uro);

            // Make both users allowed in the room
            ChatPrivateRoomUsers ura1 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.AllowedUsers.Add(ura1);
            admin.AllowedRooms.Add(ura1);
            _repository.Add(ura1);
            ChatPrivateRoomUsers ura2 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };
            room.AllowedUsers.Add(ura2);
            owner.AllowedRooms.Add(ura2);
            _repository.Add(ura2);

            // Add the admin and the owner to the room
            _repository.AddUserRoom(admin, room);
            _repository.AddUserRoom(owner, room);

            chatService.UnallowUser(admin, owner, room);

            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(owner));
            Assert.False(owner.Rooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).Contains(owner));
            Assert.False(owner.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
        }

        [Fact]
        public void AdminCanUnallowCreator()
        {
            // Create two new users and add them to the repository
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var creator = new ChatUser
            {
                Name = "foo"
            };
            _repository.Add(admin);
            _repository.Add(creator);

            // Create a new chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true,
                CreatorKeyNavigation = creator
            };
            _repository.Add(room);

            // Make both users the owners of the room
            ChatRoomOwners uro1 = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.Owners.Add(uro1);
            admin.OwnedRooms.Add(uro1);
            _repository.Add(uro1);
            ChatRoomOwners uro2 = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };
            room.Owners.Add(uro2);
            creator.OwnedRooms.Add(uro2);
            _repository.Add(uro2);

            // Make both users allowed in the room
            ChatPrivateRoomUsers ura1 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.AllowedUsers.Add(ura1);
            admin.AllowedRooms.Add(ura1);
            _repository.Add(ura1);
            ChatPrivateRoomUsers ura2 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };
            room.AllowedUsers.Add(ura2);
            creator.AllowedRooms.Add(ura2);
            _repository.Add(ura2);

            // Add the admin to the room
            _repository.AddUserRoom(admin, room);

            chatService.UnallowUser(admin, creator, room);

            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(creator));
            Assert.False(creator.Rooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).Contains(creator));
            Assert.False(creator.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
        }

        [Fact]
        public void ThrowIfOwnerTriesToUnallowAdmin()
        {
            // Create two users and add them to the repository
            var owner = new ChatUser
            {
                Name = "foo"
            };
            var admin = new ChatUser
            {
                Name = "foo2",
                IsAdmin = true
            };
            _repository.Add(owner);
            _repository.Add(admin);

            // Create a chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            // Make owner a room owner
            ChatRoomOwners uro1 = new ChatRoomOwners()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };
            room.Owners.Add(uro1);
            owner.OwnedRooms.Add(uro1);
            _repository.Add(uro1);

            //Make both users allowed in the room
            ChatPrivateRoomUsers ura1 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.AllowedUsers.Add(ura1);
            admin.AllowedRooms.Add(ura1);
            _repository.Add(ura1);
            ChatPrivateRoomUsers ura2 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };
            room.AllowedUsers.Add(ura2);
            owner.AllowedRooms.Add(ura2);
            _repository.Add(ura2);

            // Add both users to the room
            _repository.AddUserRoom(admin, room);
            _repository.AddUserRoom(owner, room);

            Assert.Throws<HubException>(() => chatService.UnallowUser(owner, admin, room));
        }

        [Fact]
        public void AdminCanUnallowAdmin()
        {
            // Create two users and add them to the repo
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var otherAdmin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            _repository.Add(admin);
            _repository.Add(otherAdmin);

            // Create a new chat room
            var room = new ChatRoom
            {
                Name = "Room",
                Private = true
            };
            _repository.Add(room);

            //Make both users allowed in the room
            ChatPrivateRoomUsers ura1 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.AllowedUsers.Add(ura1);
            admin.AllowedRooms.Add(ura1);
            _repository.Add(ura1);
            ChatPrivateRoomUsers ura2 = new ChatPrivateRoomUsers()
            {
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = otherAdmin
            };
            room.AllowedUsers.Add(ura2);
            otherAdmin.AllowedRooms.Add(ura2);
            _repository.Add(ura2);

            // Add both users to the room
            _repository.AddUserRoom(admin, room);
            _repository.AddUserRoom(otherAdmin, room);

            chatService.UnallowUser(admin, otherAdmin, room);

            Assert.False(room.Users.Select(r => r.ChatUserKeyNavigation).Contains(otherAdmin));
            Assert.False(otherAdmin.Rooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
            Assert.False(room.AllowedUsers.Select(r => r.ChatUserKeyNavigation).Contains(otherAdmin));
            Assert.False(otherAdmin.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room));
        }

        // Add Admin tests
        [Fact]
        public void ThrowsIfActingUserIsNotAdminAddAdmin()
        {
            var nonAdmin = new ChatUser
            {
                Name = "foo"
            };
            var user = new ChatUser
            {
                Name = "foo2"
            };
            _repository.Add(nonAdmin);
            _repository.Add(user);

            Assert.Throws<HubException>(() => chatService.AddAdmin(nonAdmin, user));
        }

        [Fact]
        public void MakesUserAdmin()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo2",
                IsAdmin = false
            };
            _repository.Add(admin);
            _repository.Add(user);

            chatService.AddAdmin(admin, user);

            Assert.True(user.IsAdmin);
        }

        // Remove Admin Tests
        [Fact]
        public void ThrowsIfActingUserIsNotAdminRemoveAdmin()
        {
            var nonAdmin = new ChatUser
            {
                Name = "foo",
                IsAdmin = false
            };
            var user = new ChatUser
            {
                Name = "foo2",
                IsAdmin = true
            };
            _repository.Add(nonAdmin);
            _repository.Add(user);

            Assert.Throws<HubException>(() => chatService.RemoveAdmin(nonAdmin, user));
        }

        [Fact]
        public void RemovesUserAdmin()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var user = new ChatUser
            {
                Name = "foo2",
                IsAdmin = true
            };
            _repository.Add(admin);
            _repository.Add(user);

            chatService.RemoveAdmin(admin, user);

            Assert.False(user.IsAdmin);
        }

        //  Change Welcome tests
        [Fact]
        public void ThrowsIfActingUserIsNotAdminChangeWelcome()
        {
            var nonAdmin = new ChatUser
            {
                Name = "foo",
                IsAdmin = false
            };
            var room = new ChatRoom();
            _repository.Add(nonAdmin);
            _repository.Add(room);

            Assert.Throws<HubException>(() => chatService.ChangeWelcome(nonAdmin, room, null));
        }

        [Fact]
        public void SetsRoomWelcome()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var room = new ChatRoom();
            var welcome = "bar";

            _repository.Add(admin);
            _repository.Add(room);

            chatService.ChangeWelcome(admin, room, welcome);

            Assert.Equal(welcome, room.Welcome);
        }

        [Fact]
        public void ClearsRoomWelcome()
        {
            var admin = new ChatUser
            {
                Name = "foo",
                IsAdmin = true
            };
            var room = new ChatRoom { Welcome = "bar" };

            _repository.Add(admin);
            _repository.Add(room);

            chatService.ChangeWelcome(admin, room, "");

            Assert.True(String.IsNullOrEmpty(room.Welcome));
        }
    }
}
