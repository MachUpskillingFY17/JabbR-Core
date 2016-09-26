using Xunit;
using System;
using System.Linq;
using JabbR_Core.Data.Models;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JabbR_Core.Tests.Repositories
{
    public class PersistedRepositoryTest
    {
        JabbrContext _context;
        PersistedRepository _repository;
        //DbContextOptionsBuilder _options;
        DbContextOptions<JabbrContext> _options;

        public PersistedRepositoryTest()
        {
            //IServiceCollection service = new ServiceCollection();

            //_options = new DbContextOptionsBuilder();
            _options = new DbContextOptions<JabbrContext>();

            /*string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JabbREFTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            service.AddDbContext<JabbrContext>(_options => _options.UseSqlServer(connection));

            var contextOps = new DbContextOptions<JabbrContext>();*/

            //_context = new JabbrContext(contextOps);
            _context = new JabbrContext(_options);


            _repository = new PersistedRepository(_context);
        }

       /*[Fact]
        public void GetRooms()
        {
            // Create two new chat rooms
            var room1 = new ChatRoom()
            {
                CreatorKey = 1,
                Name = "Room 1",
                Closed = false,
                Topic = "Horses"
            };

            /*var room2 = new ChatRoom()
            {
                CreatorKey = 2,
                Name = "Room 2",
                Closed = false,
                Topic = "Poetry"
            };

            //var testRooms = new List<ChatRoom>() { room1, room2};
            var testRooms = new List<ChatRoom>() { room1 };

            foreach (ChatRoom r in testRooms)
            {
                Console.WriteLine("Sample Room Names: " + r.Name);
            }

            Console.WriteLine("--------------------------------------");

            // Populate DB with new chat rooms
            _context.Add(room1);
            //_context.Add(room2);

            // Make sure repository returns the correct information
            var rooms = _repository.Rooms.ToList();

            foreach (ChatRoom r in rooms)
            {
                Console.WriteLine("Result Room Names: " + r.Name);
            }

            Assert.Equal(testRooms, rooms);

            Console.WriteLine("\tPersistedRepositoryTest.GetRooms: Complete");

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            //_context.Remove(room1);
            //_context.Remove(room2);
        }*/

        [Fact]
        //public void GetUsers(JabbrContext context)
        public void GetUsers()
        {
            //_repository = new PersistedRepository(context);

            // Create two new chat users
            var user1 = new ChatUser()
            {
                Id = "1",
                Name = "User 1",
                LastActivity = DateTime.Now
            };

            /*var user2 = new ChatUser()
            {
                Id = "2",
                Name = "User 2",
                LastActivity = DateTime.Now
            };*/

            //var testUsers = new List<ChatUser>() { user1, user2 };
            //var testUsers = new List<ChatUser>() { user1 };

            /*foreach (ChatUser u in testUsers)
            {
                Console.WriteLine("Sample User Names: " + u.Name);
            }*/
            Console.WriteLine("Sample User Names: " + user1.Name);


            Console.WriteLine("--------------------------------------");

            // Populate DB with new chat users
            //_context.Add(user1);
            _context.Users.Add(user1);
            //context.Add(user2);

            Console.WriteLine("--------------------------------------0000");


            // Make sure repository returns the correct information
            var users = _repository.Users;

            Console.WriteLine(users.Provider);

            //var users = _repository.Users.First();
            var list = users.Select(c => c).ToList();


            

            /*foreach (ChatUser u in users)
            {
                Console.WriteLine("Result User Names: " + u.Name);
            }*/
            //Console.WriteLine("Result User Names: " + users.Name);

            Console.WriteLine("Result Date Time: " + list[0].LastActivity);

            Assert.Equal(user1.Name, list[0].Name);

            Console.WriteLine("\tPersistedRepositoryTest.GetUsers: Complete");

            // FOR NOW, MAKE SURE TO DELETE THE OBJECT FROM THE DB AFTER THE TEST RUNS OTHERWISE IT WILL FAIL IF IT IS RUN TWICE
            //context.Remove(user1);
            //context.Remove(user1);
        }

        public IQueryable<ChatClient> Clients
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IQueryable<Settings> Settings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IQueryable<ChatUser> GetOnlineUsers(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatUser> GetOnlineUsers()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatUser> SearchUsers(string name)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatMessage> GetMessagesByRoom(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatMessage> GetPreviousMessages(string messageId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ChatRoom> GetAllowedRooms(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Notification> GetNotificationsByUser(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public ChatMessage GetMessageById(string id)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public ChatRoom GetRoomByName(string roomName)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByName(string userName)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByClientId(string clientId)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByLegacyIdentity(string userIdentity)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByIdentity(string providerName, string userIdentity)
        {
            throw new NotImplementedException();
        }

        public ChatUser GetUserByRequestResetPasswordId(string userName, string requestResetPasswordId)
        {
            throw new NotImplementedException();
        }

        public Notification GetNotificationById(int notificationId)
        {
            throw new NotImplementedException();
        }

        public ChatClient GetClientById(string clientId, bool includeUser = false)
        {
            throw new NotImplementedException();
        }

        public void AddUserRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatClient client)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatMessage message)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatUserIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void Add(Attachment attachment)
        {
            throw new NotImplementedException();
        }

        public void Add(Settings settings)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatRoomChatUserOwner owner)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatRoomChatUserAllowed allowed)
        {
            throw new NotImplementedException();
        }

        public void Add(ChatUserChatRooms userRoom)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatClient client)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatUser user)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatUserIdentity identity)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatRoomChatUserOwner owner)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatRoomChatUserAllowed allowed)
        {
            throw new NotImplementedException();
        }

        public void Remove(ChatUserChatRooms userRoom)
        {
            throw new NotImplementedException();
        }

        public void CommitChanges()
        {
            throw new NotImplementedException();
        }

        public bool IsUserInRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void Reload(object entity)
        {
            throw new NotImplementedException();
        }

        public void Add(Notification notification)
        {
            throw new NotImplementedException();
        }

        public void Remove(Notification notification)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
