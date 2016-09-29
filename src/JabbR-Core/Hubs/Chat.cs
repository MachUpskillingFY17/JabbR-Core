using System;
using System.Linq;
using Newtonsoft.Json;
using JabbR_Core.Models;
using JabbR_Core.Services;
using JabbR_Core.Commands;
using JabbR_Core.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Principal;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Hubs
{
    public class Chat : Hub, INotificationService
    {
        private readonly InMemoryRepository _repository;
        private readonly List<LobbyRoomViewModel> _lobbyRoomList;
        private readonly LobbyRoomViewModel _lobbyRoom;
        private readonly List<ChatRoom> _roomList;
        private readonly ChatUser _user;
        private readonly UserViewModel _userViewModel;
        private readonly RoomViewModel _roomViewModel;
        private readonly ChatRoom _room;
        private readonly List<string> _chatRooms;
        private readonly ILogger _logger;
        private readonly IChatService _service;
        private readonly ApplicationSettings _settings;
        private readonly ICache _cache;
        private readonly IRecentMessageCache _recentMessageCache;


        public Chat(
            //InMemoryRepository repository,
            //ILogger logger,
            //IChatService service
            )
        {
            _repository = new InMemoryRepository();
            _settings = new ApplicationSettings();
            _recentMessageCache = new RecentMessageCache();
            //_service = service;
            _service = new ChatService(null, _recentMessageCache, _repository,_settings);
           
            _recentMessageCache = new RecentMessageCache();
            //_cache = new ICache();

            //_repository = repository;
            _userViewModel = _repository.UserModel;
            _roomViewModel = _repository.RoomViewModel;
            _roomList = _repository.RoomList;
            _lobbyRoom = _repository.LobbyRoomView;
            _lobbyRoomList = _repository.LobbyRoomList;
            //_user = _repository.user;
            //_room = _repository.Room;
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
            // var userId = _user.Id;
            var userId = Context.User.GetUserId();

            // Try to get the user from the client state
            //ChatUser user = _user;
            ChatUser user = _repository.GetUserById(userId);
            
            //remove
            Clients.Caller.userNameChanged(user);

            //Simple test to see if server is hit from client
            Clients.Caller.logOn(new object[0], new object[0], new { TabOrder = new List<string>() });
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            string userId = Context.User.GetUserId();
            ChatUser user = _repository.VerifyUserId(userId);

            // var userId = _user.Id;
            // ChatUser user = _user;


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

        public object GetCommands()
        {
            return CommandManager.GetCommandsMetaData();
            //return CommandManager.GetCommands();
        }

        public object GetShortcuts()
        {
            return new[] {
                new { Name = "Tab or Shift + Tab", Group = "shortcut", IsKeyCombination = true, Description = LanguageResources.Client_ShortcutTabs },
                new { Name = "Alt + L", Group = "shortcut", IsKeyCombination = true, Description = LanguageResources.Client_ShortcutLobby },
                new { Name = "Alt + Number", Group = "shortcut", IsKeyCombination = true, Description = LanguageResources.Client_ShortcutSpecificTab }
            };
        }

        public async void LoadRooms(string[] roomNames)
        {
            // Can't async whenall because we'd be hitting a single 
            // EF context with multiple concurrent queries.
            foreach (var room in _roomList)
            {
                if (room == null || (room.Private && !_user.AllowedRooms.Contains(room)))
                {
                    continue;
                }
                RoomViewModel roomInfo = null;
                //var roomInfo = new RoomViewModel
                //{
                //    Name = "light_meow",
                //    Count = 1
                //};

                while (true)
                {
                    try
                    {
                        // If invoking roomLoaded fails don't get the roomInfo again
                        roomInfo = roomInfo ?? await GetRoomInfoCore(room);
                        Clients.Caller.roomLoaded(roomInfo);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ex);
                    }
                }
            }
        }

        //public void UpdateActivity()
        //{
        //    UpdateActivity(_user, _room);
        //}
        public void UpdateActivity()
        {
            string userId = Context.User.GetUserId();

            ChatUser user = _repository.GetUserById(userId);

            foreach (var room in user.Rooms)
            {
                UpdateActivity(user, room);
            }

            CheckStatus();
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
                Content = content,  // '/join light_meow'
                Room = roomName,    // 'Lobby'
            };


            return Send(message);
        }

        public bool Send(ClientMessage clientMessage)
        {
            //ChatUser user = _repository.;
            //ChatRoom room = _room;
            
            //REMOVE -- added manually to explicitly call joinRoom
            //Clients.Caller.joinRoom(user, room, new object());
            //GetRoomInfo(room.Name);

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
            //var userId = _user.Id;

            ChatUser user = _repository.VerifyUserId(userId);
            ChatRoom room = _repository.VerifyUserRoom(_cache, user, clientMessage.Room);
            //ChatUser user = _user;
            //ChatRoom room = _room;

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
            //string userId = _user.Id;

            var commandManager = new CommandManager(clientId, UserAgent, userId, room, _service, _repository, _cache, this);
            return commandManager.TryHandleCommand(command);

        }
        void INotificationService.JoinRoom(ChatUser user, ChatRoom room)
        {
            
            var userViewModel = new UserViewModel(user);
            var roomViewModel = new RoomViewModel
            {
                Name = room.Name,
                Private = room.Private,
                Welcome = room.Welcome ?? String.Empty,
                Closed = room.Closed
            };

            var isOwner = user.OwnedRooms.Contains(room);
            Clients.Caller.joinRoom(roomViewModel);

            // Tell all clients to join this room
            Clients.User(user.Id).joinRoom(roomViewModel);

            // Tell the people in this room that you've joined
            Clients.Group(room.Name).addUser(userViewModel, room.Name, isOwner);

            // Notify users of the room count change
            OnRoomChanged(room);

            foreach (var client in user.ConnectedClients)
            {
                Groups.Add(client.Id, room.Name);
            }
        }

        //void JoinRoom(ChatUser user, ChatRoom room)
        //{
        //    var userViewModel = new UserViewModel(user);
        //    var roomViewModel = new RoomViewModel
        //    {
        //        Name = room.Name,
        //        Private = room.Private,
        //        Welcome = room.Welcome ?? String.Empty,
        //        Closed = room.Closed
        //    };

        //    var isOwner = user.OwnedRooms.Contains(room);

        //     Tell all clients to join this room
        //    Clients.User(user.Id).joinRoom(roomViewModel);

        //     Tell the people in this room that you've joined
        //    Clients.Group(room.Name).addUser(userViewModel, room.Name, isOwner);

        //     Notify users of the room count change
        //    OnRoomChanged(room);

        //    foreach (var client in user.ConnectedClients)
        //    {
        //        Groups.Add(client.Id, room.Name);
        //    }
        //}

        //public void JoinRoom(ChatUser user, ChatRoom room, string inviteCode)
        //{
           
        //    // Throw if the room is private but the user isn't allowed
        //    if (room.Private)
        //    {
        //        // First, check if the invite code is correct
        //        if (!String.IsNullOrEmpty(inviteCode) && String.Equals(inviteCode, room.InviteCode, StringComparison.OrdinalIgnoreCase))
        //        {
        //            // It is, add the user to the allowed users so that future joins will work
        //            room.AllowedUsers.Add(user);
        //        }

        //        if (!room.IsUserAllowed(user))
        //        {
        //            throw new HubException(String.Format(LanguageResources.Join_LockedAccessPermission, room.Name));
        //        }
        //    }

        //    // Add this user to the room
        //    _repository.AddUserRoom(user, room);

        //    ChatUserPreferences userPreferences = user.Preferences;
        //    userPreferences.TabOrder.Add(room.Name);
        //    user.Preferences = userPreferences;

        //    // Clear the cache
        //    _cache.RemoveUserInRoom(user, room);
           
        //}

        public Task<RoomViewModel> GetRoomInfo(string roomName)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                return null;
            }

            string userId = Context.User.GetUserId();
            //string userId = _user.Id;
            ChatUser user = _repository.VerifyUserId(userId);
            //ChatUser user = _user;

            ChatRoom room = _repository.GetRoomByName(roomName);
            //ChatRoom room = _room;

            if (room == null || (room.Private && !user.AllowedRooms.Contains(room)))
            {
                return null;
            }

            return GetRoomInfoCore(room);
        }

        private async Task<RoomViewModel> GetRoomInfoCore(ChatRoom room)
        {
            var recentMessages = _recentMessageCache.GetRecentMessages(room.Name);

            //// If we haven't cached enough messages just populate it now
            if (recentMessages.Count == 0)
            {
                //var messages = await (from m in _repository.GetMessagesByRoom(room)
                //                      orderby m.When descending
                //                      select m).Take(50).ToListAsync();
                var messages = new List<ChatMessage>();

                // Reverse them since we want to get them in chronological order
                messages.Reverse();

                recentMessages = messages.Select(m => new MessageViewModel(m)).ToList();

                _recentMessageCache.Add(room.Name, recentMessages);
            }
            //REMOVE AND CHANGEEEEE
            ////Get online users through the repository
            //List<ChatUser> onlineUsers = await _repository.GetOnlineUsers(room).ToListAsync();


            return new RoomViewModel
            {
                Name = room.Name,
                Users = from u in _repository.Users
                        select new UserViewModel(u),
                //Owners = from u in room.Owners.Online()
                //         select u.Name,
                Owners = from u in room.Owners select u.Name,
                RecentMessages = recentMessages,
                Topic = room.Topic ?? string.Empty,
                Welcome = room.Welcome ?? String.Empty,
                Closed = room.Closed
                
            };
            //_roomViewModel.RecentMessages = recentMessages;
            //return _roomViewModel;
            //return new RoomViewModel
            //{
            //    Name = room.Name,
            //    Users = from u in onlineUsers
            //            select new UserViewModel(u),
            //    Owners = from u in room.Owners.Online()
            //             select u.Name,
            //    RecentMessages = recentMessages,
            //    Topic = room.Topic ?? String.Empty,
            //    Welcome = room.Welcome ?? String.Empty,
            //    Closed = room.Closed
            //};
        }


        void INotificationService.LogOn(ChatUser user, string clientId)
        {
            LogOn(user, clientId, reconnecting: true);
        }

        void INotificationService.KickUser(ChatUser targetUser, ChatRoom room, ChatUser callingUser, string reason)
        {
            var targetUserViewModel = new UserViewModel(targetUser);
            var callingUserViewModel = new UserViewModel(callingUser);

            if (String.IsNullOrWhiteSpace(reason))
            {
                reason = null;
            }

            Clients.Group(room.Name).kick(targetUserViewModel, room.Name, callingUserViewModel, reason);

            foreach (var client in targetUser.ConnectedClients)
            {
                Groups.Remove(client.Id, room.Name);
            }

            OnRoomChanged(room);
        }

        void INotificationService.OnUserCreated(ChatUser user)
        {
            // Set some client state
            Clients.Caller.name = user.Name;
            Clients.Caller.id = user.Id;
            Clients.Caller.hash = user.Hash;

            // Tell the client a user was created
            Clients.Caller.userCreated();
        }

        void INotificationService.AllowUser(ChatUser targetUser, ChatRoom targetRoom)
        {
            // Build a viewmodel for the room
            var roomViewModel = new RoomViewModel
            {
                Name = targetRoom.Name,
                Private = targetRoom.Private,
                Closed = targetRoom.Closed,
                Topic = targetRoom.Topic ?? String.Empty,
                Count = _repository.GetOnlineUsers(targetRoom).Count()
            };

            // Tell this client it's allowed.  Pass down a viewmodel so that we can add the room to the lobby.
            Clients.User(targetUser.Id).allowUser(targetRoom.Name, roomViewModel);

            // Tell the calling client the granting permission into the room was successful
            Clients.Caller.userAllowed(targetUser.Name, targetRoom.Name);
        }

        void INotificationService.UnallowUser(ChatUser targetUser, ChatRoom targetRoom, ChatUser callingUser)
        {
            // Kick the user from the room when they are unallowed
            ((INotificationService)this).KickUser(targetUser, targetRoom, callingUser, null);

            // Tell this client it's no longer allowed
            Clients.User(targetUser.Id).unallowUser(targetRoom.Name);

            // Tell the calling client the granting permission into the room was successful
            Clients.Caller.userUnallowed(targetUser.Name, targetRoom.Name);
        }

        void INotificationService.AddOwner(ChatUser targetUser, ChatRoom targetRoom)
        {
            // Tell this client it's an owner
            Clients.User(targetUser.Id).makeOwner(targetRoom.Name);

            var userViewModel = new UserViewModel(targetUser);

            // If the target user is in the target room.
            // Tell everyone in the target room that a new owner was added
            if (_repository.IsUserInRoom(_cache, targetUser, targetRoom))
            {
                Clients.Group(targetRoom.Name).addOwner(userViewModel, targetRoom.Name);
            }

            // Tell the calling client the granting of ownership was successful
            Clients.Caller.ownerMade(targetUser.Name, targetRoom.Name);
        }

        void INotificationService.RemoveOwner(ChatUser targetUser, ChatRoom targetRoom)
        {
            // Tell this client it's no longer an owner
            Clients.User(targetUser.Id).demoteOwner(targetRoom.Name);

            var userViewModel = new UserViewModel(targetUser);

            // If the target user is in the target room.
            // Tell everyone in the target room that the owner was removed
            if (_repository.IsUserInRoom(_cache, targetUser, targetRoom))
            {
                Clients.Group(targetRoom.Name).removeOwner(userViewModel, targetRoom.Name);
            }

            // Tell the calling client the removal of ownership was successful
            Clients.Caller.ownerRemoved(targetUser.Name, targetRoom.Name);
        }

        void INotificationService.ChangeGravatar(ChatUser user)
        {
            Clients.Caller.hash = user.Hash;

            // Update the calling client
            Clients.User(user.Id).gravatarChanged(user.Hash);

            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Tell all users in rooms to change the gravatar
            foreach (var room in user.Rooms)
            {
                Clients.Group(room.Name).changeGravatar(userViewModel, room.Name);
            }
        }

        void INotificationService.OnSelfMessage(ChatRoom room, ChatUser user, string content)
        {
            Clients.Group(room.Name).sendMeMessage(user.Name, content, room.Name);
        }

        void INotificationService.SendPrivateMessage(ChatUser fromUser, ChatUser toUser, string messageText)
        {
            // Send a message to the sender and the sendee
            Clients.User(fromUser.Id).sendPrivateMessage(fromUser.Name, toUser.Name, messageText);

            Clients.User(toUser.Id).sendPrivateMessage(fromUser.Name, toUser.Name, messageText);
        }

        void INotificationService.PostNotification(ChatRoom room, ChatUser user, string message)
        {
            Clients.User(user.Id).postNotification(message, room.Name);
        }

        void INotificationService.ListRooms(ChatUser user)
        {
            string userId = Context.User.GetUserId();

            var userModel = new UserViewModel(user);

            Clients.Caller.showUsersRoomList(userModel, user.Rooms.Allowed(userId).Select(r => r.Name));
        }

        void INotificationService.ListUsers()
        {
            var users = _repository.Users.Online().Select(s => s.Name).OrderBy(s => s);
            Clients.Caller.listUsers(users);
        }

        void INotificationService.ListUsers(IEnumerable<ChatUser> users)
        {
            Clients.Caller.listUsers(users.Select(s => s.Name));
        }

        void INotificationService.ListUsers(ChatRoom room, IEnumerable<string> names)
        {
            Clients.Caller.showUsersInRoom(room.Name, names);
        }

        void INotificationService.ListAllowedUsers(ChatRoom room)
        {
            Clients.Caller.listAllowedUsers(room.Name, room.Private, room.AllowedUsers.Select(s => s.Name));
        }

        void INotificationService.ListOwners(ChatRoom room)
        {
            Clients.Caller.listOwners(room.Name, room.Owners.Select(s => s.Name), room.Creator != null ? room.Creator.Name : null);
        }

        void INotificationService.LockRoom(ChatUser targetUser, ChatRoom room)
        {
            var userViewModel = new UserViewModel(targetUser);

            // Tell everyone that the room's locked
            Clients.Clients(_repository.GetAllowedClientIds(room)).lockRoom(userViewModel, room.Name, true);
            Clients.AllExcept(_repository.GetAllowedClientIds(room).ToArray()).lockRoom(userViewModel, room.Name, false);

            // Tell the caller the room was successfully locked
            Clients.Caller.roomLocked(room.Name);

            // Notify people of the change
            OnRoomChanged(room);
        }

        void INotificationService.CloseRoom(IEnumerable<ChatUser> users, ChatRoom room)
        {
            // notify all members of room that it is now closed
            foreach (var user in users)
            {
                Clients.User(user.Id).roomClosed(room.Name);
            }

            // notify everyone to update their lobby
            OnRoomChanged(room);
        }

        void INotificationService.UnCloseRoom(IEnumerable<ChatUser> users, ChatRoom room)
        {
            // notify all members of room that it is now re-opened
            foreach (var user in users)
            {
                Clients.User(user.Id).roomUnClosed(room.Name);
            }

            // notify everyone to update their lobby
            OnRoomChanged(room);
        }

        void INotificationService.LogOut(ChatUser user, string clientId)
        {
            foreach (var client in user.ConnectedClients)
            {
                DisconnectClient(client.Id);
                Clients.Client(client.Id).logOut();
            }
        }

        private void DisconnectClient(string id)
        {
            throw new NotImplementedException();
        }

        void INotificationService.ShowUserInfo(ChatUser user)
        {
            string userId = Context.User.GetUserId();

            Clients.Caller.showUserInfo(new
            {
                Name = user.Name,
                OwnedRooms = user.OwnedRooms
                    .Allowed(userId)
                    .Where(r => !r.Closed)
                    .Select(r => r.Name),
                Status = ((UserStatus)user.Status).ToString(),
                LastActivity = user.LastActivity,
                IsAfk = user.IsAfk,
                AfkNote = user.AfkNote,
                Note = user.Note,
                Hash = user.Hash,
                Rooms = user.Rooms.Allowed(userId).Select(r => r.Name)
            });
        }

        void INotificationService.ShowHelp()
        {
            Clients.Caller.showCommands();
        }

        void INotificationService.Invite(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {

            // Send the invite message to the sendee
            Clients.User(targetUser.Id).sendInvite(user.Name, targetUser.Name, targetRoom.Name);

            // Send the invite notification to the sender
            Clients.User(user.Id).sendInvite(user.Name, targetUser.Name, targetRoom.Name);
        }

        void INotificationService.NudgeUser(ChatUser user, ChatUser targetUser)
        {
            // Send a nudge message to the sender and the sendee
            Clients.User(targetUser.Id).nudge(user.Name, targetUser.Name, null);

            Clients.User(user.Id).nudge(user.Name, targetUser.Name, null);
        }

        void INotificationService.NudgeRoom(ChatRoom room, ChatUser user)
        {
            Clients.Group(room.Name).nudge(user.Name, null, room.Name);
        }

        void INotificationService.LeaveRoom(ChatUser user, ChatRoom room)
        {

            LeaveRoom(user, room);
        }

        private void LeaveRoom(ChatUser user, ChatRoom room)
        {
            var userViewModel = new UserViewModel(user);
            
            //TODO Remove explicit hub call
            Clients.Caller.leave(userViewModel, room.Name);

            Clients.Group(room.Name).leave(userViewModel, room.Name);

            foreach (var client in user.ConnectedClients)
            {
                Groups.Remove(client.Id, room.Name);
            }

            OnRoomChanged(room);
        }

        void INotificationService.OnUserNameChanged(ChatUser user, string oldUserName, string newUserName)
        {
            // Create the view model
            var userViewModel = new UserViewModel(user);


            // Tell the user's connected clients that the name changed
            Clients.User(user.Id).userNameChanged(userViewModel);

            // Notify all users in the rooms
            foreach (var room in user.Rooms)
            {
                Clients.Group(room.Name).changeUserName(oldUserName, userViewModel, room.Name);
            }
        }

        void INotificationService.ChangeAfk(ChatUser user)
        {
            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Tell all users in rooms to change the note
            foreach (var room in user.Rooms)
            {
                Clients.Group(room.Name).changeAfk(userViewModel, room.Name);
            }
        }

        void INotificationService.ChangeNote(ChatUser user)
        {
            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Tell all users in rooms to change the note
            foreach (var room in user.Rooms)
            {
                Clients.Group(room.Name).changeNote(userViewModel, room.Name);
            }
        }

        void INotificationService.ChangeFlag(ChatUser user)
        {
            bool isFlagCleared = String.IsNullOrWhiteSpace(user.Flag);

            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Update the calling client
            Clients.User(user.Id).flagChanged(isFlagCleared, userViewModel.Country);

            // Tell all users in rooms to change the flag
            foreach (var room in user.Rooms)
            {
                Clients.Group(room.Name).changeFlag(userViewModel, room.Name);
            }
        }

        void INotificationService.ChangeTopic(ChatUser user, ChatRoom room)
        {
            Clients.Group(room.Name).topicChanged(room.Name, room.Topic ?? String.Empty, user.Name);

            // trigger a lobby update
            OnRoomChanged(room);
        }

        void INotificationService.ChangeWelcome(ChatUser user, ChatRoom room)
        {
            bool isWelcomeCleared = String.IsNullOrWhiteSpace(room.Welcome);
            var parsedWelcome = room.Welcome ?? String.Empty;
            Clients.User(user.Id).welcomeChanged(isWelcomeCleared, parsedWelcome);
        }

        void INotificationService.GenerateMeme(ChatUser user, ChatRoom room, string message)
        {
            Send(message, room.Name);
        }

        void INotificationService.AddAdmin(ChatUser targetUser)
        {
            // Tell this client it's an owner
            Clients.User(targetUser.Id).makeAdmin();

            var userViewModel = new UserViewModel(targetUser);

            // Tell all users in rooms to change the admin status
            foreach (var room in targetUser.Rooms)
            {
                Clients.Group(room.Name).addAdmin(userViewModel, room.Name);
            }

            // Tell the calling client the granting of admin status was successful
            Clients.Caller.adminMade(targetUser.Name);
        }

        void INotificationService.RemoveAdmin(ChatUser targetUser)
        {
            // Tell this client it's no longer an owner
            Clients.User(targetUser.Id).demoteAdmin();

            var userViewModel = new UserViewModel(targetUser);

            // Tell all users in rooms to change the admin status
            foreach (var room in targetUser.Rooms)
            {
                Clients.Group(room.Name).removeAdmin(userViewModel, room.Name);
            }

            // Tell the calling client the removal of admin status was successful
            Clients.Caller.adminRemoved(targetUser.Name);
        }

        void INotificationService.BroadcastMessage(ChatUser user, string messageText)
        {
            // Tell all users in all rooms about this message
            foreach (var room in _repository.Rooms)
            {
                Clients.Group(room.Name).broadcastMessage(messageText, room.Name);
            }
        }

        void INotificationService.ForceUpdate()
        {
            Clients.All.forceUpdate();
        }

        private void OnRoomChanged(ChatRoom room)
        {
            var roomViewModel = new RoomViewModel
            {
                Name = room.Name,
                Private = room.Private,
                Closed = room.Closed,
                Topic = room.Topic ?? String.Empty,
                Count = _repository.GetOnlineUsers(room).Count()
            };

            // notify all clients who can see the room
            if (!room.Private)
            {
                Clients.All.updateRoom(roomViewModel);
            }
            else
            {
                Clients.Clients(_repository.GetAllowedClientIds(room)).updateRoom(roomViewModel);
            }
        }

        private ClientState GetClientState()
        {
            // New client state
            var jabbrState = GetCookieValue("jabbr.state");

            ClientState clientState = null;

            if (String.IsNullOrEmpty(jabbrState))
            {
                clientState = new ClientState();
            }
            else
            {
                clientState = JsonConvert.DeserializeObject<ClientState>(jabbrState);
            }

            return clientState;
        }

        private string GetCookieValue(string key)
        {
            //Cookie cookie;
            //Context.RequestCookies.TryGetValue(key, out cookie);
            //string value = cookie != null ? cookie.Value : null;
            //return value != null ? Uri.UnescapeDataString(value) : null;
            return "";
        }

        void INotificationService.BanUser(ChatUser targetUser, ChatUser callingUser, string reason)
        {
            var rooms = targetUser.Rooms.Select(x => x.Name).ToArray();
            var targetUserViewModel = new UserViewModel(targetUser);
            var callingUserViewModel = new UserViewModel(callingUser);

            if (String.IsNullOrWhiteSpace(reason))
            {
                reason = null;
            }

            // We send down room so that other clients can display that the user has been banned
            foreach (var room in rooms)
            {
                Clients.Group(room).ban(targetUserViewModel, room, callingUserViewModel, reason);
            }

            foreach (var client in targetUser.ConnectedClients)
            {
                foreach (var room in rooms)
                {
                    // Remove the user from this the room group so he doesn't get the general ban message
                    Groups.Remove(client.Id, room);
                }
            }
        }

        void INotificationService.UnbanUser(ChatUser targetUser)
        {
            Clients.Caller.unbanUser(new
            {
                Name = targetUser.Name
            });
        }

        void INotificationService.CheckBanned()
        {
            // Get all users that are banned
            var users = _repository.Users.Where(u => u.IsBanned)
                                         .Select(u => u.Name)
                                         .OrderBy(u => u);

            Clients.Caller.listUsers(users);
        }

        void INotificationService.CheckBanned(ChatUser user)
        {
            Clients.Caller.checkBanned(new
            {
                Name = user.Name,
                IsBanned = user.IsBanned
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }

            base.Dispose(disposing);
        }
        private void LogOn(ChatUser user, string clientId, bool reconnecting)
        {
            if (!reconnecting)
            {
                // Update the client state
                Clients.Caller.id = user.Id;
                Clients.Caller.name = user.Name;
                Clients.Caller.hash = user.Hash;
                Clients.Caller.unreadNotifications = user.Notifications.Count(n => !n.Read);
            }

            var rooms = new List<RoomViewModel>();
            var privateRooms = new List<LobbyRoomViewModel>();
            var userViewModel = new UserViewModel(user);
            var ownedRooms = user.OwnedRooms.Select(r => r.Key);

            foreach (var room in user.Rooms)
            {
                var isOwner = ownedRooms.Contains(room.Key);

                // Tell the people in this room that you've joined
                Clients.Group(room.Name).addUser(userViewModel, room.Name, isOwner);

                // Add the caller to the group so they receive messages
                Groups.Add(clientId, room.Name);

                if (!reconnecting)
                {
                    // Add to the list of room names
                    rooms.Add(new RoomViewModel
                    {
                        Name = room.Name,
                        Private = room.Private,
                        Closed = room.Closed
                    });
                }
            }


            if (!reconnecting)
            {
                foreach (var r in user.AllowedRooms)
                {
                    privateRooms.Add(new LobbyRoomViewModel
                    {
                        Name = r.Name,
                        Count = _repository.GetOnlineUsers(r).Count(),
                        Private = r.Private,
                        Closed = r.Closed,
                        Topic = r.Topic
                    });
                }

                // Initialize the chat with the rooms the user is in
                Clients.Caller.logOn(rooms, privateRooms, user.Preferences);
            }
        }


    }

}
