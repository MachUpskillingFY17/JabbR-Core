using System;
using System.Linq;
using JabbR_Core.Models;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Hubs
{

    public class Chat : Hub
    {

        private readonly InMemoryRepository _repository;
        private readonly List<LobbyRoomViewModel> _lobbyRoomList;
        private readonly LobbyRoomViewModel _lobbyRoom;
        private readonly List<ChatRoom> _roomList;
        private readonly ChatUser _user;
        private readonly List<string> _chatRooms;
        private readonly ILogger _logger;
        private readonly IChatService _service;


        public Chat(
            //InMemoryRepository repository,
            //ILogger logger,
            //IChatService service
            )
        {
            _repository = new InMemoryRepository();
            //_service = service;
            _service = new ChatService();

            //_repository = repository;
            _roomList = _repository.RoomList;
            _lobbyRoom = _repository.LobbyRoomView;
            _lobbyRoomList = _repository.LobbyRoomList;
            _user = _repository.User;
            //_chatRooms = repository.ChatRooms;
            //_logger = logger;
            //_service = service;

        }

        private string UserAgent
        {
            get
            {
                if (Context.Headers != null)
                {
                    return Context.Headers["User-Agent"];
                }
                return null;
            }
        }

        public void Join()
        {
            Join(reconnecting: false);
        }

        public void Join(bool reconnecting)
        {
            // Get the client state
            var userId = _user.Id;
            //var userId = Context.User.GetUserId();

            // Try to get the user from the client state
            ChatUser user = _user;
            //ChatUser user = _repository.GetUserById(userId);

            //Simple test to see if server is hit from client
            Clients.Caller.logOn(new object[0], new object[0], new { TabOrder = new List<string>() });
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            // string userId = Context.User.GetUserId();
            // ChatUser user = _repository.VerifyUserId(userId);

            var userId = _user.Id;
            ChatUser user = _user;


            var room = new LobbyRoomViewModel
            {
                Name = user.Name,
                Count = '1',
                //    Count = r.Users.Count(u => u.Status != (int)UserStatus.Offline),
                Private = _lobbyRoom.Private,
                Closed = _lobbyRoom.Closed,
                Topic = _lobbyRoom.Topic 
            };

            _lobbyRoomList.Add(_lobbyRoom);
            return _lobbyRoomList;
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
            foreach (var room in _roomList)
            {
                //if (room == null || (room.Private && !_user.AllowedRooms.Contains(room)))
                //{
                //    continue;
                //}


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
            UpdateActivity(_user, _chatRooms);
        }

        private void UpdateActivity(ChatUser user, List<string> rooms)
        {

        }
    }
}
