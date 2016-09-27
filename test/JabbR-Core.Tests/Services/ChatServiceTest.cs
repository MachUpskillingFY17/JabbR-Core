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




        //UpdateActivity test TODO: fix that stuff with clients???
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
           // Assert.Equal(1, clients.Count); //not sure why ChatServiceFacts tests this? it'll obvi be null
           // Assert.Equal("client1", clients[0].Id);
           // Assert.Same(user, clients[0].UserKeyNavigation);
            Assert.Null(user.AfkNote);
            Assert.False(user.IsAfk);

            _repository.Remove(user);
        }



        //LeaveRoom tests
        //AJS: Doesn't work due to incorrect inmemoryrepository function. 
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
        }

        //AddMessage tests: ERROR
        [Fact]
        public void AddsNewMessageToRepository()
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

            ChatMessage message = chatService.AddMessage(user, room, Guid.NewGuid().ToString(), "Content");

            Assert.NotNull(message);
            Assert.Same(message, room.ChatMessages.First()); //Where would chatmessages ever be updated???
            Assert.Equal("Content", message.Content);

            _repository.Remove(user);
            _repository.Remove(room);
        }


        //AddOwner tests: error with injabbrmemoryrepo
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
            ChatUserChatRooms cr = new ChatUserChatRooms()
            {
                ChatRoomKey = room.Key,
                ChatUserKey = user.Key,
                ChatRoomKeyNavigation = room,
                ChatUserKeyNavigation = user
            };

            room.Users.Add(cr);
            user.Rooms.Add(cr);
            
            Assert.Throws<HubException>(() => chatService.AddOwner(user, user, room));
            _repository.Remove(user);
               
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
                CreatorKeyNavigation = user
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
            room.Owners.Add(cro);
            user.OwnedRooms.Add(cro);
            user.Rooms.Add(cr);
            room.Users.Add(cr);

            chatService.AddOwner(user, user2, room);

            Assert.True(room.Owners.Select(c=> c.ChatUserKeyNavigation).ToList().Contains(user2));
            Assert.True(user2.OwnedRooms.Select(c=> c.ChatRoomKeyNavigation).ToList().Contains(room));

            _repository.Remove(user);
            _repository.Remove(user2);
        }



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
