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

            ChatRoomChatUserAllowed cr = new ChatRoomChatUserAllowed() {
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
        //user.ConnectedClients isn't getting updated and will be null. Functionality to come?
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

            ChatUserChatRooms cr = new ChatUserChatRooms()
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

        //AddMessage tests: FAILS
        //Right now room.ChatMessages isn't beeing updated-issue with cache?
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
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

            ChatUserChatRooms cr = new ChatUserChatRooms()
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

            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            ChatRoomChatUserOwner cro = new ChatRoomChatUserOwner()
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
            ChatRoomChatUserOwner cro = new ChatRoomChatUserOwner()
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            ChatRoomChatUserOwner cro = new ChatRoomChatUserOwner()
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
            ChatRoomChatUserAllowed userAllowed = new ChatRoomChatUserAllowed()
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = oldOwner.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = oldOwner
            };
            ChatRoomChatUserOwner cro = new ChatRoomChatUserOwner()
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };
            ChatUserChatRooms crtrgt = new ChatUserChatRooms()
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = unspecialUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = unspecialUser
            };
            ChatUserChatRooms cr2 = new ChatUserChatRooms()
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
            ChatRoomChatUserOwner cro = new ChatRoomChatUserOwner()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = unspecialUser.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = unspecialUser
            };
            ChatRoomChatUserOwner cro2 = new ChatRoomChatUserOwner()
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = admin.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = admin
            };
            ChatUserChatRooms cr2 = new ChatUserChatRooms()
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
            ChatRoomChatUserOwner cro = new ChatRoomChatUserOwner()
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


        //

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
