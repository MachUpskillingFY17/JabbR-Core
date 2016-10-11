//using System.Collections.Generic;
//using System.Security.Principal;
//using JabbR_Core.Infrastructure;
//using JabbR_Core.Data.Models;
//using JabbR_Core.Data.Repositories;
//using JabbR_Core.Hubs;
//using JabbR_Core.Services;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.AspNetCore.SignalR.Hubs;
//using Moq;

//namespace JabbR.Test
//{
//    public class ChatFacts
//    {
//        public static TestableChat GetTestableChat(string clientId, ChatUser user)
//        {
//            return GetTestableChat(clientId, user, new Dictionary<string, Cookie>());
//        }

//        public static TestableChat GetTestableChat(string connectionId, ChatUser user, IDictionary<string, Cookie> cookies)
//        {
//            // setup things needed for chat
//            var repository = new InMemoryRepository();
//            //var resourceProcessor = new Mock<ContentProviderProcessor>();
//            var chatService = new Mock<IChatService>();
//            var connection = new Mock<IConnection>();
//            var settings = new ApplicationSettings();
//            var mockPipeline = new Mock<IHubPipelineInvoker>();

//            // add user to repository
//            repository.Add(user);

//            // create testable chat
//            var chat = new TestableChat(chatService, repository, connection, settings);
//            var mockedConnectionObject = chat.MockedConnection.Object;

//            chat.Clients = new HubConnectionContext(mockPipeline.Object, mockedConnectionObject, "Chat", connectionId);

//            var principal = new Mock<IPrincipal>();

//            var request = new Mock<IRequest>();
//            request.Setup(m => m.Cookies).Returns(cookies);
//            request.Setup(m => m.User).Returns(principal.Object);

//            // setup context
//            chat.Context = new HubCallerContext(request.Object, connectionId);

//            return chat;
//        }

//        public class TestableChat : Chat
//        {
//            //public Mock<ContentProviderProcessor> MockedResourceProcessor { get; private set; }
//            public Mock<IChatService> MockedChatService { get; private set; }
//            public IJabbrRepository Repository { get; private set; }
//            public Mock<IConnection> MockedConnection { get; private set; }
//            public ApplicationSettings Settings { get; private set; }

//            public TestableChat(Mock<IChatService> mockedChatService, IJabbrRepository repository, Mock<IConnection> connection, ApplicationSettings settings)
//                : base(mockedChatService.Object,
//                       new Mock<IRecentMessageCache>().Object, 
//                       repository, 
//                       new Mock<ICache>().Object, 
//                       new Mock<ILogger>().Object,
//                       settings)
//            {
//                MockedChatService = mockedChatService;
//                Repository = repository;
//                MockedConnection = connection;
//                Settings = settings;
//            }
//            //public TestableChat(Mock<ContentProviderProcessor> mockedResourceProcessor, Mock<IChatService> mockedChatService, IJabbrRepository repository, Mock<IConnection> connection, ApplicationSettings settings)
//            //    : base(mockedResourceProcessor.Object,
//            //           mockedChatService.Object,
//            //           new Mock<IRecentMessageCache>().Object, 
//            //           repository, 
//            //           new Mock<ICache>().Object, 
//            //           new Mock<ILogger>().Object,
//            //           settings)
//            //{
//            //    MockedResourceProcessor = mockedResourceProcessor;
//            //    MockedChatService = mockedChatService;
//            //    Repository = repository;
//            //    MockedConnection = connection;
//            //    Settings = settings;
//            //}
//        }
//    }
//}
