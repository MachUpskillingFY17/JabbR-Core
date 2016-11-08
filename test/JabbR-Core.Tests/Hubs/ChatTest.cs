using Moq;
using Xunit;
using System;
using JabbR_Core.Hubs;
using System.Collections;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using System.Security.Claims;
using JabbR_Core.Data.Models;
using JabbR_Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR.Hubs;
using System.Runtime.InteropServices.ComTypes;

//====================================
// project.json nuget package
// "moq.netcore": "4.4.0-beta8"
//====================================

namespace JabbR_Core.Tests.Hubs
{
    public class ChatTest
    {
        private readonly Chat _chat;

        private ICache _cache;
        private JabbrContext _context;
        private ChatService _chatService;
        private IJabbrRepository _repository;
        private IRecentMessageCache _recentMessageCache;
        private OptionsManager<ApplicationSettings> _settings;
        public ChatTest()
        {
            // Fetch new instances of the required objects
            _context = new JabbrContext(new DbContextOptions<JabbrContext>());
            _repository = new InMemoryRepository(_context);

            _cache = new DefaultCache();
            _recentMessageCache = new RecentMessageCache();
            _settings = new OptionsManager<ApplicationSettings>(new List<IConfigureOptions<ApplicationSettings>>() { });

            _chatService = new ChatService(_cache, _recentMessageCache, _repository);
             
            // Create Mocks of the objects being passed into SignalR
            var request = new Mock<HttpRequest>();
            var connection = new Mock<IConnection>();
            var pipeline = new Mock<IHubPipelineInvoker>();

            // Taken from normal JabbR-Core execution
            var connectionId = "79482a87-8d16-42bc-b5ce-1fb7b309ad1e";

            // Establish new Chat hub with normal SignalR connection + pipeline
            var chat = new TestableChat(_repository, _settings, _chatService, connection);
            chat.Clients = new HubConnectionContext(pipeline.Object, chat.MockConnection.Object, "Chat", connectionId);

            // Include required claims with request for authentication
            // Adam's LoginFakerMiddleware runs but doesn't establish Hub context
            // so we can put the same code here to establish an identity
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, "James"));
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, "provider"));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "identity"));
            claims.Add(new Claim(ClaimTypes.Email, "james@no.com"));
            claims.Add(new Claim(JabbRClaimTypes.Identifier, "2"));

            var claimsIdentity = new ClaimsIdentity(claims, Constants.JabbRAuthType);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            ChatUser user = new ChatUser()
            {
                Name = "James",
                Email = "james@no.com",
                Id = "2",
                Identity = claimsIdentity.ToString()
            };

            // Add to repository for methods that perform user verification
            _repository.Add(user);

            // Establish request properties here, investigate query string
            request.Setup(r => r.Cookies).Returns(new Mock<IRequestCookieCollection>().Object);
            request.Setup(r => r.HttpContext.User).Returns(claimsPrincipal);

            // Register the real SignalR context to the TestableChat.
            chat.Context = new HubCallerContext(request.Object, connectionId);
            chat.Groups = new GroupManager(connection.Object, "mygroup");
            
            // Instantiate Chat hub.
            _chat = chat;
        }

         // Join()

        // GetRooms()
        // Changes to InMemoryRepository are causing these to fail, commenting out until InMem is fixed
        [Fact]
        public void GetRoomsNotNull()
        {
            Assert.NotEqual(null, _chat.GetRooms());
        }

        [Fact]
        public void GetCommandsNotNullNotEmpty()
        {
            Assert.NotEqual(null, _chat.GetCommands());
            Assert.NotEmpty(_chat.GetCommands());
        }

        [Fact]
        public void GetShortcutsNotNullNotEmpty()
        {
            Assert.NotEqual(null, _chat.GetShortcuts());
            Assert.NotEmpty((IEnumerable)_chat.GetShortcuts());
        }

        // Context is null when called from external Hub, cannot test yet.
        [Fact]
        public void UpdateAcivityNoException()
        {
            _chat.UpdateActivity();
        }

        [Fact]
        public void CreateRoomReturnsTrue()
        {
            Assert.True(_chat.Send("/create MyRoom", null));
        }

        [Fact]
        public void CreateRoomExceptionAlreadyExists()
        {
            _chat.Send("/create MyRoom", null);

            Assert.Throws<HubException>(() => _chat.Send("/create MyRoom", null));
        }

        [Fact]
        public void LeaveRoomReturnsTrue()
        {
            // Must create and join room before leaving it
            _chat.Send("/create MyRoom", null);

            Assert.True(_chat.Send("/leave MyRoom", null));
        }

        [Fact]
        public void LeaveRoomExceptionDoesNotExist()
        {
            // Must create and join room before leaving it
            _chat.Send("/create MyRoom", null);

            Assert.Throws<HubException>(() => _chat.Send("/leave AnotherRoom", null));
        }

        [Fact]
        public void LeaveRoomExceptionAlreadyLeft()
        {
            // Must create and join room before leaving it
            _chat.Send("/create MyRoom", null);
            _chat.Send("/leave MyRoom", null);

            // Returns true and notification is shown
            Assert.True(_chat.Send("/leave MyRoom", null));
        }

        [Fact]
        public void OpenRoomExceptionRoomAlreadyOpen()
        {
            _chat.Send("/create MyRoomA", null);
            _chat.Send("/create MyRoomB", null);

            // MyRoomA is already open
            Assert.Throws<HubException>(() => _chat.Send("/open MyRoomA", null));
        }

        [Fact]
        public void SendMessageReturnsTrue()
        {
            // Create and join room
            _chat.Send("/create MyRoom", null);

            Assert.True(_chat.Send("Hey there!", "MyRoom"));
        }

        [Fact]
        public void SendMultipleMessageReturnsTrue()
        {
            // Create and join room
            _chat.Send("/create MyRoom", null);

            Assert.True(_chat.Send("Hey there!", "MyRoom"));
            Assert.True(_chat.Send("Hello again", "MyRoom"));
            Assert.True(_chat.Send("Hi friends", "MyRoom"));
        }

        [Fact]
        public void JoinWhenMultpleRoomsReturnsTrue()
        {
            _chat.Send("/create MyRoomA", null);
            _chat.Send("/create MyRoomB", null);

            Assert.True(_chat.Send("/join MyRoomA", null));
        }

        [Fact]
        public void JoinCurrentRoomReturnsTrue()
        {
            _chat.Send("/create MyRoomA", null);
            _chat.Send("/create MyRoomB", null);

            Assert.True(_chat.Send("/join MyRoomB", null));
        }

        [Fact]
        public void JoinRoomExceptionDoesNotExist()
        {
            _chat.Send("/create MyRoom", null);

            Assert.Throws<HubException>(() => _chat.Send("/join AnotherRoom", null));
        }

        [Fact]
        public void JoinRoomAfterLeavingReturnsTrue()
        {
            _chat.Send("/create MyRoom", null);
            _chat.Send("/leave MyRoom", null);

            Assert.True(_chat.Send("/join MyRoom", null));
        }

        // send message to roomA while in roomB ??

        [Fact]
        public void CreateRoomMessageThenLeaveReturnsTrue()
        {
            Assert.True(_chat.Send("/create MyRoomA", null));
            Assert.True(_chat.Send("Hey friends", "MyRoomA"));
            Assert.True(_chat.Send("/create MyRoomB", null));
            Assert.True(_chat.Send("Hello again", "MyRoomB"));
            Assert.True(_chat.Send("/leave MyRoomA", null));
            Assert.True(_chat.Send("/leave MyRoomB", null));
        }

        [Fact]
        public void SendMessageAfterLeavingException()
        {
            _chat.Send("/create MyRoomA", null);
            _chat.Send("Hey friends", "MyRoomA");
            _chat.Send("/leave MyRoomA", null);

            // Correct behaviour is returning true when not in the room, but maybe that should change
            Assert.Throws<HubException>(() => _chat.Send("Hello again", "MyRoomA"));
        }

        [Fact]
        public void SendMessageFromAnotherRoomTrue()
        {
            _chat.Send("/create MyRoomA", null);
            _chat.Send("/create MyRoomB", null);

            // Correct behaviour, but maybe returning True when sent from another room is bad?
            Assert.True(_chat.Send("Hey friends", "MyRoomA"));
        }

        // GetCommands()
        // Implementation empty

        // GetShortcuts()
        // Should populate with hardcoded shortcut keys
        [Fact]
        public void GetShortcutsNotNull()
        {
            Assert.NotEqual(null, _chat.GetShortcuts());
            Console.WriteLine("\tChatTest.GetShortcutsNotNull: Complete");
        }

        // Basic test to see if any exceptions are hit
        [Fact]
        public void SendAcceptsCommandParams()
        {
            Assert.True(_chat.Send("/create foo", null));

            var content = "/join foo";
            Assert.True(_chat.Send(content, null));
            Console.WriteLine("\tChatTest.SendAcceptsParams: Complete");
        }

        [Fact]
        public void SendParamsTooLongException()
        {
            var content = "/join foo";
            var roomName = "bar";
            var _settings = new ApplicationSettings();
            //_settings.MaxMessageLength = 3;
            Assert.Throws<HubException>(() => _chat.Send(content, roomName));
            Console.WriteLine("\tChatTest.SendAcceptsParams: Complete");
        }

        // TryHandleCommand(string command, string room)
        [Fact]
        public void TryHandleCommandValid()
        {
            Assert.True(_chat.Send("/create foo", null));
            var command = "/join foo";
            var roomName = "bar";
            Assert.True(_chat.TryHandleCommand(command, roomName));
        }

        [Fact]
        public void TryHandleCommandInValid()
        {
            var command = "join foo";
            var roomName = "bar";
            Assert.False(_chat.TryHandleCommand(command, roomName));
        }
        
        [Fact]
        public void SendMessageRoomNullException()
        {
            _chat.Send("/create MyRoomA", null);

            // Cannot send a message when no room specified, even when in a room
            Assert.Throws<HubException>(() => _chat.Send("Hey friends", null));
        }

        [Fact]
        public void SendMessageExceptionNoRoom()
        {
            // Cannot send a message when no rooms exist
            Assert.Throws<HubException>(() => _chat.Send("Hey friends", null));
        }

        [Fact]
        public void SendMessageExceptionRoomDoesNotExist()
        {
            // Cannot send a message when no rooms exist
            Assert.Throws<HubException>(() => _chat.Send("Hey friends", "MyRoomA"));
        }

    }

    public class TestableChat : Chat
    {
        public IJabbrRepository Repository { get; set; }
        public ChatService ChatService { get; set; }
        public Mock<IConnection> MockConnection { get; set; }
        public OptionsManager<ApplicationSettings> Settings { get; set; }

        public TestableChat(IJabbrRepository repository, OptionsManager<ApplicationSettings> settings, ChatService chatService, Mock<IConnection> mockConnection) 
            : base(repository, settings, new Mock<RecentMessageCache>().Object, chatService, null)
        {
            Repository = repository;
            ChatService = chatService;
            MockConnection = mockConnection;
            Settings = settings;
        }
    }
}
