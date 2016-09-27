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
        //Addroom 

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



        //
        public ChatRoom AddRoom(ChatUser user, string roomName)
        {
            throw new NotImplementedException();
        }

        public void AddAdmin(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public ChatClient AddClient(ChatUser user, string clientId, string userAgent)
        {
            throw new NotImplementedException();
        }

        public ChatMessage AddMessage(string userId, string roomName, string url)
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

        public void AddOwner(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
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

        public void JoinRoom(ChatUser user, ChatRoom room, string inviteCode)
        {
            throw new NotImplementedException();
        }

        public void KickUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void LeaveRoom(ChatUser user, ChatRoom room)
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

        public void UpdateActivity(ChatUser user, string clientId, string userAgent)
        {
            throw new NotImplementedException();
        }
    }
}
