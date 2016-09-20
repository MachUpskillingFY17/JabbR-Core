using Xunit;
using System;
using System.Linq;
using JabbR_Core.Hubs;
using JabbR_Core.Models;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace JabbR_Core.Tests.Hubs
{
    public class ChatTest
    {
        private Chat _chat;

        public ChatTest()
        {
            _chat = new Chat();
        }

        // Tests
        [Fact]
        public void GetRoomsNotNull()
        {
            Assert.NotEqual(null, _chat.GetRooms());
            Console.WriteLine("\tChatTest.GetRoomsNotNull: Complete");
        }

        [Fact]
        public void GetRoomsNoDuplicates()
        {
            var result = true;
            var rooms = _chat.GetRooms();
            var list = new List<string>();

            for(int i=0;i<rooms.Count;i++) 
            {
                if(!list.Contains(rooms[i].Name))
                {
                    list.Add(rooms[i].Name);
                }
                else
                {
                    result = false;
                    break;
                }
            }

            Assert.True(result);
            Console.WriteLine("\tChatTest.GetRoomsNoDuplicates: Complete");
        }

    }
}
