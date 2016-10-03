using Xunit;
using System;
using JabbR_Core.Hubs;
using JabbR_Core.ViewModels;
using System.Collections.Generic;
using JabbR_Core.Services;
using Microsoft.Extensions.Options;
using JabbR_Core.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;

//using Moq;
//using Microsoft.AspNetCore.SignalR.Hubs;
//using System.Dynamic;

namespace JabbR_Core.Tests.Hubs
{
    public class ChatTest
    {
        private Chat _chat;

        private ICache _cache;
        private JabbrContext _context;
        private IChatService _chatService;
        private InMemoryRepository _repository;
        private IRecentMessageCache _recentMessageCache;
        private IOptions<ApplicationSettings> _settings;

        public ChatTest()
        {
            // Fetch new instances of the required objects
            GetCleanRepository();

            // Settings
            _settings = new OptionsManager<ApplicationSettings>(new List<IConfigureOptions<ApplicationSettings>>() { });

            // Cache
            _recentMessageCache = new RecentMessageCache();
            _cache = new DefaultCache();

            // Chat Service
            _chatService = new ChatService(_cache, _recentMessageCache, _repository);

            // Instantiate Chat hub.
            //_chat = new Chat(_repository, _settings, _recentMessageCache, _chatService);
        }

        //[Fact]
        //public void ChatHubIsMockable()
        //{
        //    bool sendCalled = false;

        //    _chat = new Chat(_repository, _settings, _recentMessageCache, _chatService);

        //    var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
        //    var mockContext = new Mock<HubCallerContext>();
            

        //    _chat.Clients = mockClients.Object;
        //    _chat.Context = mockContext.Object;

        //    dynamic all = new ExpandoObject();
        //    all.broadcastMessage = new Action<string, string>((name, message) => {
        //        sendCalled = true;
        //    });

        //    mockClients.Setup(m => m.All).Returns((ExpandoObject)all);
        //    _chat.Send("TestContent", "TestRoomname");
        //    Assert.True(sendCalled);
        //}

        // Use this method at the beginning of tests to make sure that 
        // values in old tests won't impact the current one
        public void GetCleanRepository()
        {
            // Repository
            _context = new JabbrContext(new DbContextOptions<JabbrContext>());
            _repository = new InMemoryRepository(_context);
        }

        // Tests
        [Fact]
        public void GetRoomsNotNull()
        {
            Assert.NotEqual(null, _chat.GetRooms());
        }

        [Fact]
        public void GetCommandsNotNullNotEmpty()
        {
            Assert.NotEqual(null, _chat.GetCommands());
            Assert.NotEmpty((IEnumerable)_chat.GetCommands());
        }

        [Fact]
        public void GetShortcutsNotNullNotEmpty()
        {
            Assert.NotEqual(null, _chat.GetShortcuts());
            Assert.NotEmpty((IEnumerable)_chat.GetShortcuts());
        }

        // Context is null when called from external Hub, cannot test yet.
        //[Fact]
        //public void UpdateActivitySomething()
        //{
        //    _chat.UpdateActivity();
        //}

        // This throws an InvalidOperationException because actions that send through
        // SignalR hubs that are not instantiated in the SignalR pipeline are forbidden
        // Cannot test other operations until we change this behaviour.
        //[Fact]
        //public void LoadRoomsInvalidOperationException()
        //{
        //    GetCleanRepository(); 

        //    _repository.RoomList.Add(new Models.ChatRoom
        //    {
        //        Name = "ChatRoom One",
        //        Topic = "One"
        //    });

        //    // Need to reinstantiate _chat so that in its constructor the _repository.RoomList
        //    // assignment is up to date with the RoomList changes we added above.
        //    _chat = new Chat(_repository, _settings, _recentMessageCache, _chatService);

        //    Assert.Throws<InvalidOperationException>(() => _chat.LoadRooms());
        //}


        // Wouldn't this be better as a repository test?
        [Fact]
        public void AddRoomsVerification()
        {
            GetCleanRepository();

            var rooms = _chat.GetRooms();

            Assert.Empty(rooms);

            var room = new LobbyRoomViewModel()
            {
                Name = "Room",
                Topic = "JabbR"
            };

            rooms.Add(room);

            Assert.Contains(room, _chat.GetRooms());
        }

        [Fact]
        public void SendTrue()
        {
            Assert.True(_chat.Send("hello", null));
        }

    }
}
