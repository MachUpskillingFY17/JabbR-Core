using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using JabbR_Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Entity;

namespace JabbR_Core.Hubs
{

    public class Chat : Hub
    {
        public List<string> rooms;
        public void Join()
        {
            //Simple test to see if server is hit from client
            Clients.Caller.logOn(new object[0], new object[0], new { TabOrder = new List<string>() });
            //Clients.Caller.GetRooms(rooms);
            //Clients.Caller.GetCommands(new object[0]);
            //Clients.Caller.GetShortCuts(new object[0]);
            //Clients.Caller.LoadRooms(new object[0]);
            //Clients.Caller.updateActivity(new UserData() {
            //    name = "heather",
            //    hash = null,
            //    owner = null,
            //    active = null,
            //    noteClass = null,
            //    note = null,
            //    flagClass = null,
            //    flag = null,
            //    country = null,
            //    lastActive = "2016-08-23 00:26:35.713",
            //    timeAgo = null,
            //    admin = true,
            //    afk = true,
            // }
            //);
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            //List<string> rooms;
            UserData user = new UserData()
            {
                name = "heather",
                hash = null,
                owner = null,
                active = null,
                noteClass = null,
                note = null,
                flagClass = null,
                flag = null,
                country = null,
                lastActive = "2016-08-23 00:26:35.713",
                timeAgo = null,
                admin = true,
                afk = true,
            };

            var hack = new List<LobbyRoomViewModel>();
            
            hack.Add(new LobbyRoomViewModel
            {
                Name = user.name,
                Count = 1,
                Private = false,
                Closed = false,
                Topic = null
            });


            return hack;
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
        public void LoadRooms()
        {

        }

        public void updateActivity()
        {
            //string userId = "1";

            UserData user = new UserData()
                {
                    name = "heather",
                    hash = null,
                    owner = null,
                    active = null,
                    noteClass = null,
                    note = null,
                    flagClass = null,
                    flag = null,
                    country = null,
                    lastActive = "2016-08-23 00:26:35.713",
                    timeAgo = null,
                    admin = true,
                    afk = true,
                };

            //foreach (var room in user.Rooms)
            //{
                UpdateActivity(user, rooms);
            //}

            //CheckStatus();
        }

        private void UpdateActivity(UserData user, List<string> rooms)
        {
            
        }
    }
}
