using System;
using JabbR_Core.Models;
using JabbR_Core.Services;
using JabbR_Core.ViewModels;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.SignalR;

namespace JabbR_Core.Hubs
{
    public class Chat : Hub
    {
        private readonly InMemoryRepository _repository;
        private readonly List<LobbyRoomViewModel> _lobbyRoomList;
        private readonly LobbyRoomViewModel _lobbyRoom;
        private readonly List<ChatRoom> _roomList;
        private readonly ChatUser _user;
        private readonly ChatRoom _room;
        private readonly List<string> _chatRooms;
        private readonly ILogger _logger;
        private readonly IChatService _service;
        private readonly ApplicationSettings _settings;
        private readonly ICache _cache;


        public Chat(
            //InMemoryRepository repository,
            //ILogger logger,
            //IChatService service
            )
        {
            _repository = new InMemoryRepository();
            //_service = service;
            _service = new ChatService();
            _settings = new ApplicationSettings();
            //_cache = new ICache();

            //_repository = repository;
            _roomList = _repository.RoomList;
            _lobbyRoom = _repository.LobbyRoomView;
            _lobbyRoomList = _repository.LobbyRoomList;
            _user = _repository.User;
            _room = _repository.Room;
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
            UpdateActivity(_user, _room);
        }

        private void UpdateActivity(ChatUser user, ChatRoom room)
        {
            UpdateActivity(user);

            OnUpdateActivity(user, room);
        }

        private void OnUpdateActivity(ChatUser user, ChatRoom room)
        {
            var userViewModel = new UserViewModel(user);
            Clients.Group(room.Name).updateActivity(userViewModel, room.Name);
        }

        private void UpdateActivity(ChatUser user)
        {
            _service.UpdateActivity(user, Context.ConnectionId, UserAgent);

            _repository.CommitChanges();
        }

        public bool Send(string content, string roomName)
        {
            var message = new ClientMessage
            {
                Content = content,
                Room = roomName,
            };

            return Send(message);
        }

        public bool Send(ClientMessage clientMessage)
        {
            CheckStatus();

            //reject it if it's too long
            if (_settings.MaxMessageLength > 0 && clientMessage.Content.Length > _settings.MaxMessageLength)
            {
                throw new HubException(String.Format(LanguageResources.SendMessageTooLong, _settings.MaxMessageLength));
            }

            //See if this is a valid command (starts with /)
            if (TryHandleCommand(clientMessage.Content, clientMessage.Room))
            {
                return true;
            }

            var userId = Context.User.GetUserId();

            //ChatUser user = _repository.VerifyUserId(userId);
            //ChatRoom room = _repository.VerifyUserRoom(_cache, user, clientMessage.Room);
            ChatUser user = _user;
            ChatRoom room = _room;


            if (room == null || (room.Private && !user.AllowedRooms.Contains(room)))
            {
                return false;
            }

            // REVIEW: Is it better to use the extension method room.EnsureOpen here?
            if (room.Closed)
            {
                throw new HubException(String.Format(LanguageResources.SendMessageRoomClosed, clientMessage.Room));
            }

            // Update activity *after* ensuring the user, this forces them to be active
            UpdateActivity(user, room);

            // Create a true unique id and save the message to the db
            string id = Guid.NewGuid().ToString("d");
            ChatMessage chatMessage = _service.AddMessage(user, room, id, clientMessage.Content);
            _repository.CommitChanges();


            var messageViewModel = new MessageViewModel(chatMessage);

            if (clientMessage.Id == null)
            {
                // If the client didn't generate an id for the message then just
                // send it to everyone. The assumption is that the client has some ui
                // that it wanted to update immediately showing the message and
                // then when the actual message is roundtripped it would "solidify it".
                Clients.Group(room.Name).addMessage(messageViewModel, room.Name);
            }
            else
            {
                // If the client did set an id then we need to give everyone the real id first
                Clients.OthersInGroup(room.Name).addMessage(messageViewModel, room.Name);

                // Now tell the caller to replace the message
                Clients.Caller.replaceMessage(clientMessage.Id, messageViewModel, room.Name);
            }

            // Add mentions
            //AddMentions(chatMessage);

            //var urls = UrlExtractor.ExtractUrls(chatMessage.Content);
            //if (urls.Count > 0)
            //{
            //    _resourceProcessor.ProcessUrls(urls, Clients, room.Name, chatMessage.Id);
            //}

            return true;
        }

        private void CheckStatus()
        {
            if (OutOfSync)
            {
                Clients.Caller.outOfSync();
            }
        }

        private bool OutOfSync
        {
            get
            {
                string version = Context.QueryString["version"];

                if (String.IsNullOrEmpty(version))
                {
                    return true;
                }

                //return new Version(version) != Constants.JabbRVersion;
                return false;
            }
        }

        private bool TryHandleCommand(string command, string room)
        {
            string clientId = Context.ConnectionId;
            string userId = Context.User.GetUserId();

            //var commandManager = new CommandManager(clientId, UserAgent, userId, room, _service, _repository, _cache, this);
            //return commandManager.TryHandleCommand(command
            return true;
        }
    }
}
