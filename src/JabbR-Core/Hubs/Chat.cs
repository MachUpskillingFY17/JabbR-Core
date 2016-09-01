using System;
using System.Linq;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Hubs
{

    public class Chat : Hub
    {
        public List<string> Rooms;

        public void Join()
        {
            //Simple test to see if server is hit from client
            Clients.Caller.logOn(new object[0], new object[0], new { TabOrder = new List<string>() });
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            //List<string> rooms;
            var user = new UserData()
            {
                Name = "light_meow",
                LastActivity = "2016-08-23 00:26:35.713",
                Admin = true,
                Afk = true,
        };

            var rooms = new List<LobbyRoomViewModel>
            {
                new LobbyRoomViewModel
                {
                    Name = user.Name,
                    Count = 1,
                }
            };


            return rooms;
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

        public void UpdateActivity()
        {
            var user = new UserData()
                {
                    Name = "light_meow",
                    Admin = true,
                    Afk = true,
                };

                UpdateActivity(user, Rooms);
        }

        private void UpdateActivity(UserData user, List<string> rooms)
        {
            
        }
    }
}
