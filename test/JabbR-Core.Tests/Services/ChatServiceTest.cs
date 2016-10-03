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

namespace JabbR_Core.Tests.Services
{
    public class ChatServiceTest
    {
        ChatService chatService;
        IJabbrRepository _repository;
        ICache _cache;
        IRecentMessageCache _recentMessageCache;
        JabbrContext _context;
        DbContextOptions<JabbrContext> _options;
        MemoryCache _memCache;
        IOptions<MemoryCacheOptions> optionsAccessor;

        public ChatServiceTest()
        {
            // Set up connection string with local DB
            //_options = new DbContextOptions<JabbrContext>();
            //_context = new JabbrContext(_options);
            _repository = new InMemoryRepository();
            
            _recentMessageCache = new RecentMessageCache();

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

            _repository.Remove(user);

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

            _repository.Remove(user);
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

            _repository.Remove(user);
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

            _repository.Remove(user);

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
            _repository.Remove(user);
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
            Assert.True(room.Owners.Select(c=> c.ChatUserKeyNavigation).Contains(user));
            Assert.Same(room.CreatorKeyNavigation, user);
            Assert.True(user.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(user);
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

            chatService.JoinRoom(user, room, null);

            Assert.True(user.Rooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.Users.Select(c=>c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(user);
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

            UserRoomAllowed cr = new UserRoomAllowed() {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatUserKeyNavigation = user,
                ChatRoomKeyNavigation = room
            };

            room.AllowedUsers.Add(cr);
            user.AllowedRooms.Add(cr);

            chatService.JoinRoom(user, room, null);

            Assert.True(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(user);
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

            Assert.Throws<HubException>(() => chatService.JoinRoom(user, room, null));

            _repository.Remove(user);
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

            chatService.JoinRoom(user, room, null);

            Assert.True(user.Rooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.Users.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(user);
        }




        //UpdateActivity tests: FAILS
        //user.ConnectedClients isn't getting updated and will be null until DI in ChatService is implemented
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

            _repository.Remove(user);
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

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Users.Add(cr);
            user.Rooms.Add(cr);

            chatService.LeaveRoom(user, room);

            Assert.False(user.Rooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(user);
            _repository.Remove(room);
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
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Users.Add(cr);
            user.Rooms.Add(cr);

            ChatMessage message = chatService.AddMessage(user, room, Guid.NewGuid().ToString(), "Content");

            Assert.NotNull(message);
            Assert.Same(message, room.ChatMessages.First()); 
            Assert.Equal("Content", message.Content);

            _repository.Remove(user);
            _repository.Remove(room);
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

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            _repository.Add(cr);

            room.Users.Add(cr);
            user.Rooms.Add(cr);

            Assert.Throws<HubException>(() => chatService.AddOwner(user, user, room));

            _repository.Remove(user);
            _repository.Remove(room);
            _repository.Remove(cr);
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

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Users.Add(cr);
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);

            Assert.Throws<HubException>(() => chatService.AddOwner(user, user, room));
            _repository.Remove(user);
        }

        [Fact]
        public void MakesUserOwner()
        {
            // User authorizing the new owner, this user is already a room owner
            var oldOwner = new ChatUser
            {
                Name = "foo",
                Key = 1
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
                Creator_Key = oldOwner.Key
            };

            // Now that both the original owner and room have been created, add the owner relationship
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            room.Owners.Add(cro);
            oldOwner.OwnedRooms.Add(cro);

            // Try to add a new owner
            chatService.AddOwner(oldOwner, newOwner, room);

            // Verify owner was added
            Assert.True(room.Owners.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(newOwner));
            Assert.True(newOwner.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            // Clean up data from the repository
            _repository.Remove(oldOwner);
            _repository.Remove(newOwner);
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
            // Now that both the original owner and room have been created, add the owner relationship
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };

            room.Owners.Add(cro);
            oldOwner.OwnedRooms.Add(cro);
            oldOwner.Rooms.Add(cr);
            room.Users.Add(cr);

            //Allow new owner into room
            UserRoomAllowed userAllowed = new UserRoomAllowed()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = newOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = newOwner
            };

            newOwner.AllowedRooms.Add(userAllowed);
            room.AllowedUsers.Add(userAllowed);

            chatService.AddOwner(oldOwner, newOwner, room);


            Assert.True(room.Owners.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(newOwner));
            Assert.True(newOwner.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(oldOwner);
            _repository.Remove(newOwner);
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

            // Now that both the original owner and room have been created, add the owner relationship
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };

            room.Owners.Add(cro);
            oldOwner.OwnedRooms.Add(cro);
            oldOwner.Rooms.Add(cr);
            room.Users.Add(cr);


            chatService.AddOwner(oldOwner, allowedUsr, room);

            Assert.True(allowedUsr.AllowedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(allowedUsr));
            Assert.True(room.Owners.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(allowedUsr));
            Assert.True(allowedUsr.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(oldOwner);
            _repository.Remove(allowedUsr);
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

            //create user/room relationship 
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };

            admin.Rooms.Add(cr);
            room.Users.Add(cr);


            chatService.AddOwner(admin, user, room);

            Assert.True(room.Owners.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));
            Assert.True(user.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(admin);
            _repository.Remove(user);
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
            };

            room.CreatorKeyNavigation = user;

            //Add both users to room
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom crtrgt = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = targetUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };

            user.Rooms.Add(cr);
            targetUser.Rooms.Add(crtrgt);
            room.Users.Add(cr);
            room.Users.Add(crtrgt);


            Assert.Throws<HubException>(() => chatService.RemoveOwner(user, targetUser, room));

            _repository.Remove(user);
            _repository.Remove(targetUser);
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
            //add unspecialUser and user relationships to room
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = unspecialUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = unspecialUser
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            unspecialUser.Rooms.Add(cr);
            user.Rooms.Add(cr2);

            room.Users.Add(cr);
            room.Users.Add(cr2);

            //Make both owners
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = unspecialUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = unspecialUser
            };
            UserRoomOwner cro2 = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Owners.Add(cro);
            unspecialUser.OwnedRooms.Add(cro);

            room.Owners.Add(cro2);
            user.OwnedRooms.Add(cro2);

            Assert.Throws<HubException>(() => chatService.RemoveOwner(unspecialUser, user, room));

            _repository.Remove(unspecialUser);
            _repository.Remove(user);

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
            //Create user/room relationship 
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            admin.Rooms.Add(cr);
            user.Rooms.Add(cr2);

            room.Users.Add(cr);
            room.Users.Add(cr2);

            //Create owner
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            
            chatService.RemoveOwner(admin, user, room);

            Assert.False(room.Owners.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));
            Assert.False(user.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(admin);
            _repository.Remove(user);
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
            //Add owner and user/room relationships
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);
            room.Users.Add(cr);


            Assert.Throws<HubException>(() => chatService.KickUser(user, user, room));

            _repository.Remove(user);
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

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = targetUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };

            user.Rooms.Add(cr);
            targetUser.Rooms.Add(cr2);
            room.Users.Add(cr);
            room.Users.Add(cr2);
            
            Assert.Throws<HubException>(() => chatService.KickUser(user, targetUser, room));

            _repository.Remove(user);
            _repository.Remove(targetUser);
        }

        [Fact]
        public void ThrowsIfTargetUserNotInRoom()
        {
            var repository = new InMemoryRepository();
            var user = new ChatUser
            {
                Name = "foo"
            };

            var targetUser = new ChatUser
            {
                Name = "foo2"
            };

            repository.Add(user);
            repository.Add(targetUser);
            var room = new ChatRoom
            {
                Name = "Room",
                CreatorKeyNavigation = user
            };

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);
            user.Rooms.Add(cr);
            room.Users.Add(cr);
            
            Assert.Throws<HubException>(() => chatService.KickUser(user, targetUser, room));
            repository.Add(user);
            repository.Add(targetUser);

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
            //Add user/room and owner relationships
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = targetUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoomOwner cro2 = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = targetUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };

            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);

            targetUser.OwnedRooms.Add(cro2);
            room.Owners.Add(cro2);

            user.Rooms.Add(cr);
            targetUser.Rooms.Add(cr2);
            room.Users.Add(cr);
            room.Users.Add(cr2);
            
            Assert.Throws<HubException>(() => chatService.KickUser(user, targetUser, room));
            _repository.Remove(user);
            _repository.Remove(targetUser);
        }

        [Fact]
        public void DoesNotThrowIfCreatorKicksOwner()
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

            //create relationships
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoomOwner cro2 = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = targetUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = targetUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = targetUser
            };

            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);

            targetUser.OwnedRooms.Add(cro2);
            room.Owners.Add(cro2);

            user.Rooms.Add(cr);
            targetUser.Rooms.Add(cr2);
            room.Users.Add(cr);
            room.Users.Add(cr2);

            chatService.KickUser(user, targetUser, room);

            Assert.False(targetUser.Rooms.Select(c=>c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c=>c.ChatUserKeyNavigation).ToList().Contains(targetUser));

            _repository.Remove(user);
            _repository.Remove(targetUser);
        }
        
        [Fact]
        public void AdminCanKickUser()
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

            //add user/room relationships 
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            admin.Rooms.Add(cr2);
            user.Rooms.Add(cr);
            room.Users.Add(cr2);
            room.Users.Add(cr);
            
            chatService.KickUser(admin, user, room);

            Assert.False(user.Rooms.Select(c=>c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(admin);
            _repository.Remove(user);

        }
        
        [Fact]
        public void DoesNotThrowIfAdminKicksOwner()
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
                Name = "Room"
            };
            //Add relationships
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
           
            user.OwnedRooms.Add(cro);
            room.Owners.Add(cro);

            admin.Rooms.Add(cr2);
            user.Rooms.Add(cr);
            room.Users.Add(cr2);
            room.Users.Add(cr);

           
            chatService.KickUser(admin, user, room);

            Assert.False(user.Rooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(admin);
            _repository.Remove(user);
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
            //create relationships
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = creator.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = creator.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };

            creator.OwnedRooms.Add(cro);
            room.Owners.Add(cro);

            admin.Rooms.Add(cr2);
            creator.Rooms.Add(cr);
            room.Users.Add(cr2);
            room.Users.Add(cr);
            
            chatService.KickUser(admin, creator, room);

            Assert.False(creator.Rooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(creator));

            _repository.Remove(admin);
            _repository.Remove(creator);

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

            //create relationships
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = owner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = owner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = owner
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };

            owner.OwnedRooms.Add(cro);
            room.Owners.Add(cro);

            admin.Rooms.Add(cr2);
            owner.Rooms.Add(cr);
            room.Users.Add(cr2);
            room.Users.Add(cr);

            Assert.Throws<HubException>(() => chatService.KickUser(owner, admin, room));
            _repository.Remove(admin);
            _repository.Remove(owner);
        }

        [Fact]
        public void AdminCanKickAdmin()
        {
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

            var room = new ChatRoom
            {
                Name = "Room"
            };
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            UserRoom cr2 = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = otherAdmin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = otherAdmin
            };
            admin.Rooms.Add(cr);
            otherAdmin.Rooms.Add(cr2);
            room.Users.Add(cr);
            room.Users.Add(cr2);
            
            chatService.KickUser(admin, otherAdmin, room);

            Assert.False(otherAdmin.Rooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.False(room.Users.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(otherAdmin));

            _repository.Remove(admin);
            _repository.Remove(otherAdmin);
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

            _repository.Remove(user);
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

            _repository.Remove(user);
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

            //add relationships 
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Users.Add(cr);
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);

            chatService.LockRoom(user, room);

            Assert.True(room.Private);
            Assert.True(user.AllowedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(user);
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
            //Relationship ALL THE THINGS
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoomAllowed cra = new UserRoomAllowed()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            room.AllowedUsers.Add(cra);
            user.AllowedRooms.Add(cra);
            user.Rooms.Add(cr);
            room.Users.Add(cr);

            Assert.Throws<HubException>(() => chatService.LockRoom(user, room));

            _repository.Remove(user);
        }

        [Fact]
        public void LocksRoom()
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
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);
            room.Users.Add(cr);
            
            chatService.LockRoom(user, room);

            Assert.True(room.Private);
            Assert.True(user.AllowedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user));

            _repository.Remove(user);
        }

        [Fact]
        public void MakesAllUsersAllowed()
        {
            var creator = new ChatUser
            {
                Name = "foo"
            };
            //Online and offline users
            var users = Enumerable.Range(0, 5).Select(i => new ChatUser
            {
                Name = "user_" + i
            }).ToList();

            var offlineUsers = Enumerable.Range(6, 10).Select(i => new ChatUser
            {
                Name = "user_" + i,
                Status = (int)UserStatus.Offline
            }).ToList();

            var room = new ChatRoom
            {
                Name = "room",
                CreatorKeyNavigation = creator
            };

            //Relationship
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = creator.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = creator
            };

            room.Owners.Add(cro);
            creator.OwnedRooms.Add(cro);

            _repository.Add(room);

            foreach (var u in users)
            {
                UserRoom ur = new UserRoom()
                {
                    ChatRoomKey = room.Key,
                    ChatUserKey = u.Key,
                    ChatRoomKeyNavigation = room,
                    ChatUserKeyNavigation = u
                };
                room.Users.Add(ur);
                u.Rooms.Add(ur);
                _repository.Add(u);
            }
            foreach (var u in offlineUsers)
            {
                UserRoom ur = new UserRoom()
                {
                    ChatRoomKey = room.Key,
                    ChatUserKey = u.Key,
                    ChatRoomKeyNavigation = room,
                    ChatUserKeyNavigation = u
                };
                room.Users.Add(ur);
                u.Rooms.Add(ur);
                _repository.Add(u);
            }
          
            chatService.LockRoom(creator, room);

            foreach (var u in users)
            {
                Assert.True(u.AllowedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
                Assert.True(room.AllowedUsers.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(u));
                _repository.Remove(u);

            }

            foreach (var u in offlineUsers)
            {
                Assert.False(u.AllowedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));
                Assert.False(room.AllowedUsers.Select(c=>c.ChatUserKeyNavigation).ToList().Contains(u));
                _repository.Remove(u);

            }

            _repository.Remove(room);

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

            UserRoom cr = new UserRoom()
            { 
                ChatRoomKey = room.Key, 
                ChatUserKey = admin.Key, 
                ChatRoomKeyNavigation = room, 
                ChatUserKeyNavigation = admin 
            }; 
            room.Users.Add(cr); 
            admin.Rooms.Add(cr);           

            chatService.LockRoom(admin, room);  

            Assert.True(room.Private);
            Assert.True(admin.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));
            Assert.True(room.AllowedUsers.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(admin));  

            _repository.Remove(admin); 
        }
        //AllowUser tests

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

            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Users.Add(cr);
            user.Rooms.Add(cr);
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);


            Assert.Throws<HubException>(() => chatService.AllowUser(user, user2, room));

            _repository.Remove(user);
            _repository.Remove(user2);

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

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            room.Users.Add(cr);
            user.Rooms.Add(cr);


            Assert.Throws<HubException>(() => chatService.AllowUser(user, user, room));

            _repository.Remove(user);
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
            //Add relationships
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            UserRoomAllowed cra = new UserRoomAllowed()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user2.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user2
            };

            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Users.Add(cr);
            room.AllowedUsers.Add(cra);
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);
            user2.AllowedRooms.Add(cra);

            Assert.Throws<HubException>(() => chatService.AllowUser(user, user2, room));
            _repository.Remove(user);
            _repository.Remove(user2);
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

            //Add relationships
            UserRoomOwner cro = new UserRoomOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);
            room.Users.Add(cr);

            chatService.AllowUser(user, user2, room);

            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(user2));
            Assert.True(user2.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(user);
            _repository.Remove(user2);
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

            //add relationship
            UserRoom cr = new UserRoom()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            room.Users.Add(cr);
            admin.Rooms.Add(cr);


            chatService.AllowUser(admin, admin, room);

            Assert.True(room.AllowedUsers.Select(c => c.ChatUserKeyNavigation).ToList().Contains(admin));
            Assert.True(admin.AllowedRooms.Select(c => c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(admin);
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


        //rest of functions to test
        public void AddAdmin(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public ChatClient AddClient(ChatUser user, string clientId, string userAgent)
        {
            throw new NotImplementedException();
        }

   

        public ChatMessage AddMessage(ChatUser user, ChatRoom room, string id, string content)
        {
            throw new NotImplementedException();
        }

        public void AddNotification(ChatUser mentionedUser, ChatMessage message, ChatRoom room, bool markAsRead)
        {
            throw new NotImplementedException();
        }

        public void AllowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void AppendMessage(string id, string content)
        {
            throw new NotImplementedException();
        }

        public void BanUser(ChatUser callingUser, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public void ChangeTopic(ChatUser user, ChatRoom room, string newTopic)
        {
            throw new NotImplementedException();
        }

        public void ChangeWelcome(ChatUser user, ChatRoom room, string newWelcome)
        {
            throw new NotImplementedException();
        }

        public void CloseRoom(ChatUser user, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public string DisconnectClient(string clientId)
        {
            throw new NotImplementedException();
        }

        

        public void KickUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }



        public void LockRoom(ChatUser user, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void OpenRoom(ChatUser user, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void RemoveAdmin(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public void RemoveOwner(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void SetInviteCode(ChatUser user, ChatRoom room, string inviteCode)
        {
            throw new NotImplementedException();
        }

        public void UnallowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void UnbanUser(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

       
    }
}
