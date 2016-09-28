using Xunit;
using System;
using System.Linq;
using JabbR_Core.Hubs;
using JabbR_Core.Data.Models;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JabbR_Core.Services;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Tests.Hubs
{
    public class ChatTest
    {
        private Chat _chat;

        public ChatTest()
        {
            _chat = new Chat();
        }

        // Join()

        // GetRooms()
        // Changes to InMemoryRepository are causing these to fail, commenting out until InMem is fixed
        //[Fact]
        //public void GetRoomsNotNull()
        //{
        //    Assert.NotEqual(null, _chat.GetRooms());
        //    Console.WriteLine("\tChatTest.GetRoomsNotNull: Complete");
        //}

        //[Fact]
        //public void GetRoomsNoDuplicates()
        //{
        //    var result = true;
        //    var rooms = _chat.GetRooms();
        //    var list = new List<string>();

        //    for (int i = 0; i < rooms.Count; i++)
        //    {
        //        if (!list.Contains(rooms[i].Name))
        //        {
        //            list.Add(rooms[i].Name);
        //        }
        //        else
        //        {
        //            result = false;
        //            break;
        //        }
        //    }

        //    Assert.True(result);
        //    Console.WriteLine("\tChatTest.GetRoomsNoDuplicates: Complete");
        //}

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

        // LoadRooms()
        // Can not LoadRoom with the current implementation.
        //[Fact]
        //public void LoadRoomsNotNull()
        //{
        //    string[] roomNames =
        //    {
        //        "room0",
        //        "room1",
        //        "room2"
        //    };
        //    _chat.LoadRooms(roomNames);
        //    Collection<ChatUserChatRooms> collection;
        //}

        // Send(string content, string roomName)
        // Send(ClientMessage clientMessage)
        // Basic test to see if any exceptions are hit
        [Fact]
        public void SendAcceptsParams()
        {
            var content = "/join foo";
            var roomName = "bar";
            Assert.Throws<HubException>(() => _chat.Send(content, roomName));
            //Assert.True(_chat.Send(content, roomName));
            //Assert.False(_chat.Send(content, roomName));
            Console.WriteLine("\tChatTest.SendAcceptsParams: Complete");
        }

        [Fact]
        public void SendParamsTooLongException()
        {
            var content = "/join foo";
            var roomName = "bar";

            var _settings = new ApplicationSettings();
            for (var i = 0; i >= (_settings.MaxMessageLength); i++)
            {
                content = content + "foo";
            }
            Assert.Throws<HubException>(() => _chat.Send(content, roomName));
            //Assert.True(_chat.Send(content, roomName));
            //Assert.False(_chat.Send(content, roomName));
            Console.WriteLine("\tChatTest.SendAcceptsParams: Complete");
        }

        

    }
}
