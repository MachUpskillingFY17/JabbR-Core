using System;
using JabbR_Core.Models;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Hubs
{

    public class Chat : Hub
    {
        // Mock model instances to pass into Hub methods
        public List<string> ChatRooms { get; set; }
        public ChatUser User { get; set; }
        public string RoomNames { get; set; }
        public UserViewModel UserModel { get; set; }

        // Mock List for LoadRooms()
        public ChatRoom Room { get; set; }
        public List<ChatRoom> RoomList { get; set; }

        // Mock List for GetRoom()
        public List<LobbyRoomViewModel> LobbyRoomList { get; set; }
        public LobbyRoomViewModel LobbyRoomView { get; set; }

        // Constructor populates mock data
        public Chat()
        { 
            // populate ChatUser
            User = new ChatUser
            {
                Name = "user1",
                LastActivity = Convert.ToDateTime("2016-08-23 00:26:35.713"),
                IsAdmin = true,
                IsAfk = true
            };

            // instantiate UserViewModel object from User
            UserModel = new UserViewModel(User);

            // populate ChatRoom and RoomList
            Room = new ChatRoom {Name = "light_meow"};
            RoomList = new List<ChatRoom> {Room};
            

            // populate RoomView
            LobbyRoomView = new LobbyRoomViewModel
            {
                Name = Room.Name,
                Count = 1,
            };
            // Add RoomView to RoomList
            LobbyRoomList = new List<LobbyRoomViewModel> {LobbyRoomView};
        }

        public void Join()
        {
            //Simple test to see if server is hit from client
            Clients.Caller.logOn(new object[0], new object[0], new { TabOrder = new List<string>() });
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            return LobbyRoomList;
        }

        public void GetCommands()
        {
           
        }

        public object GetShortcuts()
        {
            return new[] {
                new { Name = "Tab or Shift + Tab", Group = "shortcut", IsKeyCombination = true, Description = LanguageResources.Client_ShortcutTabs },
                new { Name = "Alt + L", Group = "shortcut", IsKeyCombination = true, Description = LanguageResources.Client_ShortcutLobby },
                new { Name = "Alt + Number", Group = "shortcut", IsKeyCombination = true, Description = LanguageResources.Client_ShortcutSpecificTab }
            };
        }

        public void LoadRooms(string[] roomNames)
        {
            // Can't async whenall because we'd be hitting a single 
            // EF context with multiple concurrent queries.
            foreach (var room in RoomList)
            {
                if (room == null || (room.Private && !User.AllowedRooms.Contains(room)))
                {
                    continue;
                }


                var roomInfo = new RoomViewModel
                {
                    Name = "light_meow"
                };

                //while (true)
                //{
                //    try
                //    {
                //        // If invoking roomLoaded fails don't get the roomInfo again
                //        // roomInfo = roomInfo ?? await GetRoomInfoCore(room);
                //        Clients.Caller.roomLoaded(roomInfo);
                //        break;
                //    }
                //    catch (Exception ex)
                //    {
                //        // logger.Log(ex);
                //    }
                //}
            }
        }

        public void UpdateActivity()
        {
                UpdateActivity(User, ChatRooms);
        }

        private void UpdateActivity(ChatUser user, List<string> rooms)
        {
            
        }
    }
}
