using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using JabbR_Core.ViewModels;

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
            UserData user = new UserData()
            {
                Name = "soft_meow",
                Hash = null,
                Owner = null,
                Active = null,
                NoteClass = null,
                Note = null,
                FlagClass = null,
                Flag = null,
                Country = null,
                LastActive = "2016-08-23 00:26:35.713",
                TimeAgo = null,
                Admin = true,
                Afk = true,
            };

            var hack = new List<LobbyRoomViewModel>();
            
            hack.Add(new LobbyRoomViewModel
            {
                Name = user.Name,
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
            UserData user = new UserData()
                {
                    Name = "soft_meow",
                    Hash = null,
                    Owner = null,
                    Active = null,
                    NoteClass = null,
                    Note = null,
                    FlagClass = null,
                    Flag = null,
                    Country = null,
                    LastActive = "2016-08-23 00:26:35.713",
                    TimeAgo = null,
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
