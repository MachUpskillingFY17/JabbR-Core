﻿using System;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using JabbR_Core.Services;
using JabbR_Core.Commands;
using JabbR_Core.ViewModels;
using JabbR_Core.Data.Models;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using JabbR_Core.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace JabbR_Core.Hubs
{
    public class Chat : Hub, INotificationService
    {
        // Never assigned to, always null
        private readonly ICache _cache;
        private readonly ILogger _logger;

        private readonly IChatService _chatService;
        private readonly ApplicationSettings _settings;
        private readonly IJabbrRepository _repository;
        private readonly IRecentMessageCache _recentMessageCache;

        public Chat(
            IJabbrRepository repository,
            IOptions<ApplicationSettings> settings,
            IRecentMessageCache recentMessageCache,
            IChatService chatService)
        {
            // Request the injected object instances
            _repository = repository;
            _chatService = chatService;
            _recentMessageCache = recentMessageCache;
            _settings = settings.Value;
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
            var userId = Context.User.GetUserId();

            // Try to get the user from the client state
            ChatUser user = _repository.GetUserById(userId);

            // This function is being manually called here to establish
            // your identity to SignalR and update the UI to match. In 
            // original JabbR it isn't called explicitly anywhere, so 
            // something about the natural authentication data flow 
            // establishes this in SignalR for us. For now, call explicitly
            //Delete this in the future (when auth is setup properly)
            Clients.Caller.userNameChanged(user);

            // Pass the list of rooms & owned rooms to the logOn function.
            //var rooms = _repository.Rooms.ToArray();
            //var myRooms = _repository.GetOwnedRooms(user).ToList();
            List<ChatRoom> rooms = new List<ChatRoom>();
            List<ChatRoom> myRooms = new List<ChatRoom>();

            Clients.Caller.logOn(rooms, myRooms, new { TabOrder = new List<string>() });
        }

        public List<LobbyRoomViewModel> GetRooms()
        {
            //return _lobbyRoomList;

            return _repository.Rooms.Select(r => new LobbyRoomViewModel()
            {
                Name = r.Name,
                Count = r.Users.Count,
                Private = r.Private,
                Closed = r.Closed,
                Topic = r.Topic
            }).ToList();
        }

        public IEnumerable<CommandMetaData> GetCommands()
        {
            return CommandManager.GetCommandsMetaData();
        }

        // More specific return type? Object[]? or cast to Array?
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
            string userId = Context.User.GetUserId();
            ChatUser user = _repository.VerifyUserId(userId);

            // Can't async whenall because we'd be hitting a single 
            // EF context with multiple concurrent queries.
            var rooms = _repository.Rooms
                                   .Where(r => roomNames.Contains(r.Name)).ToList();
            foreach (var room in rooms)
            {
                if (room == null || (room.Private && !user.AllowedRooms.Select(u => u.ChatRoomKeyNavigation).Contains(room)))
                {
                    continue;
                }

                RoomViewModel roomInfo = null;
                while (true)
                {
                    try
                    {
                        // If invoking roomLoaded fails don't get the roomInfo again
                        roomInfo = roomInfo ?? GetRoomInfoCore(room);
                        Clients.Caller.roomLoaded(roomInfo);
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Logger is null
                        //_logger.Log(ex);
                    }
                }
            }
        }

        public void UpdateActivity()
        {
            string userId = Context.User.GetUserId();

            ChatUser user = _repository.GetUserById(userId);

            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation).ToList())
            {
                UpdateActivity(user, room);
            }

            //CheckStatus();
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
            _chatService.UpdateActivity(user, Context.ConnectionId, UserAgent);

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
            //Commented out for resting purposes
            // TODO: set env variable
            //CheckStatus();

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

            ChatUser user = _repository.VerifyUserId(userId);

            // this line breaks when we message in a new room
            ChatRoom room = _repository.VerifyUserRoom(_cache, user, clientMessage.Room);

            if (room == null || (room.Private && user.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).ToList().Contains(room)))
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

            // Ensure the message is logged
            ChatMessage chatMessage = _chatService.AddMessage(user, room, id, clientMessage.Content);
            room.ChatMessages.Add(chatMessage);

            // Save changes
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

        public bool TryHandleCommand(string command, string room)
        {
            string clientId = Context.ConnectionId;
            string userId = Context.User.GetUserId();
            var commandManager = new CommandManager(clientId, UserAgent, userId, room, _chatService, _repository, _cache, this);
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

            var isOwner = user.OwnedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room);
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

        public RoomViewModel GetRoomInfo(string roomName)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                return null;
            }

            string userId = Context.User.GetUserId();

            ChatUser user = _repository.VerifyUserId(userId);

            ChatRoom room = _repository.GetRoomByName(roomName);

            if (room == null || (room.Private && !user.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).Contains(room)))
            {
                return null;
            }

            return GetRoomInfoCore(room);
            //return new Task<RoomViewModel>(() => thing);
        }

        private RoomViewModel GetRoomInfoCore(ChatRoom room)
        {
            var recentMessages = _recentMessageCache.GetRecentMessages(room.Name);

            // If we haven't cached enough messages just populate it now
            if (recentMessages.Count == 0)
            {
                var messages = _repository.GetMessagesByRoom(room)
                    .Take(50)
                    .OrderBy(o => o.When)
                    .ToList();

                recentMessages = messages.Select(m => new MessageViewModel(m)).ToList();

                _recentMessageCache.Add(room.Name, recentMessages);
            }

            List<ChatUser> onlineUsers = _repository.GetOnlineUsers(room).ToList();

            return new RoomViewModel
            {
                Name = room.Name,
                Users = from u in onlineUsers
                        select new UserViewModel(u),
                Owners = _repository.GetRoomOwners(room).Online().Select(n => n.Name),
                RecentMessages = recentMessages,
                Topic = room.Topic ?? string.Empty,
                Welcome = room.Welcome ?? String.Empty,
                Closed = room.Closed
            };
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
            //Clients.Caller.id = user.Id;
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
            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation))
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

            Clients.Caller.showUsersRoomList(userModel, user.Rooms.Select(r => r.ChatRoomKeyNavigation).Allowed(userId).Select(r => r.Name));
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
            Clients.Caller.listAllowedUsers(room.Name, room.Private, room.AllowedUsers.Select(s => s.ChatUserKeyNavigation.Name));
        }

        void INotificationService.ListOwners(ChatRoom room)
        {
            Clients.Caller.listOwners(room.Name, room.Owners.Select(s => s.ChatUserKeyNavigation.Name), room.CreatorKeyNavigation != null ? room.CreatorKeyNavigation.Name : null);
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
            Clients.Caller.roomClosed(room.Name);
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
                    .Select(r => r.ChatRoomKeyNavigation)
                    .Allowed(userId)
                    .Where(r => !r.Closed)
                    .Select(r => r.Name),
                Status = ((UserStatus)user.Status).ToString(),
                LastActivity = user.LastActivity,
                IsAfk = user.IsAfk,
                AfkNote = user.AfkNote,
                Note = user.Note,
                Hash = user.Hash,
                Rooms = user.Rooms.Select(r => r.ChatRoomKeyNavigation).Allowed(userId).Select(r => r.Name)
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
            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation))
            {
                Clients.Group(room.Name).changeUserName(oldUserName, userViewModel, room.Name);
            }
        }

        void INotificationService.ChangeAfk(ChatUser user)
        {
            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Tell all users in rooms to change the note
            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation))
            {
                Clients.Group(room.Name).changeAfk(userViewModel, room.Name);
            }
        }

        void INotificationService.ChangeNote(ChatUser user)
        {
            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Tell all users in rooms to change the note
            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation))
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
            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation))
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
            foreach (var room in targetUser.Rooms.Select(u => u.ChatRoomKeyNavigation))
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
            foreach (var room in targetUser.Rooms.Select(u => u.ChatRoomKeyNavigation))
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
            var rooms = targetUser.Rooms.Select(x => x.ChatRoomKeyNavigation.Name).ToArray();
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
                // Let the DI Container handle disposing the repo
                //_repository.Dispose();
            }

            base.Dispose(disposing);
        }

        private void LogOn(ChatUser user, string clientId, bool reconnecting)
        {
            if (!reconnecting)
            {
                // Update the client state
                //Clients.Caller.id = user.Id;
                Clients.Caller.name = user.Name;
                Clients.Caller.hash = user.Hash;
                Clients.Caller.unreadNotifications = user.Notifications.Count(n => !n.Read);
            }

            var rooms = new List<RoomViewModel>();
            var privateRooms = new List<LobbyRoomViewModel>();
            var userViewModel = new UserViewModel(user);
            var ownedRooms = user.OwnedRooms.Select(r => r.ChatRoomKey);

            foreach (var room in user.Rooms.Select(u => u.ChatRoomKeyNavigation))
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
                foreach (var r in user.AllowedRooms.Select(r => r.ChatRoomKeyNavigation).ToList())
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
