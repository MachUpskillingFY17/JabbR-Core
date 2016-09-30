using Xunit;
using System;
using System.Linq;
using JabbR_Core.Hubs;
using JabbR_Core.Models;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using JabbR_Core.Services;
using Microsoft.Extensions.Options;
using JabbR_Core.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Tests.Hubs
{
    public class ChatTest
    {
        private Chat _chat;

        private ICache _cache;
        private JabbrContext _context;
        private IChatService _chatService;
        private IJabbrRepository _repository;
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
            _chat = new Chat(_repository, _settings, _recentMessageCache, _chatService);
        }

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
            GetCleanRepository();
            Assert.NotEqual(null, _chat.GetRooms());
            Console.WriteLine("\tChatTest.GetRoomsNotNull: Complete");
        }

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

            Assert.Contains(room, rooms);

            Console.WriteLine($"{this.GetType().ToString()}.AddRoomsVerification: Complete");
        }

    }
}
