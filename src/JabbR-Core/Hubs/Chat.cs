using JabbR_Core.Models;
using JabbR_Core.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Hubs
{

    public class Chat : Hub
    {
        public List<string> ChatRooms;
        //List<string> rooms;
        public ChatUser User;

        public Chat()
        {
            User = new ChatUser();
            User.Name = "light_meow";
            User.Admin = true;
            User.Afk = true;
        }

        public void Join()
        {
            //Simple test to see if server is hit from client
            Clients.Caller.logOn(new object[0], new object[0], new { TabOrder = new List<string>() });
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            var rooms = new List<LobbyRoomViewModel>();
            
            rooms.Add(new LobbyRoomViewModel
            {
                Name = User.Name,
                Count = 1,
            });

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
                UpdateActivity(User, ChatRooms);
        }

        private void UpdateActivity(ChatUser user, List<string> rooms)
        {
            
        }
    }
}
