using System;
using System.Linq;
using JabbR_Core.Hubs;
using JabbR_Core.ViewModels;
using JabbR_Core.Data.Models;
using JabbR_Core.Data.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace JabbR_Core.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IJabbrRepository _repository;

        public ChatNotificationService(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public void OnUserNameChanged(ChatUser user, string oldUserName, string newUserName)
        {
            // Create the view model
            var userViewModel = new UserViewModel(user);

            // Tell the user's connected clients that the name changed
            //var clients = _repository.Clients.Where(c => c.UserId == user.Id).ToList();
            //foreach (var client in clients)
            //{
            //    HubContext.Clients.Client(client.Id).userNameChanged(userViewModel);
            //}

            // Notify all users in the rooms
            foreach (var room in user.Rooms.Select(r => r.ChatRoomKeyNavigation).ToList())
            {
                HubContext.Clients.Group(room.Name).changeUserName(oldUserName, userViewModel, room.Name);
            }
        }

        public void UpdateUnreadMentions(ChatUser mentionedUser, int unread)
        {
            foreach (var client in mentionedUser.ConnectedClients)
            {
                HubContext.Clients.Client(client.Id).updateUnreadNotifications(unread);
            }
        }

        private IHubContext _hubContext;
        private IHubContext HubContext
        {
            get
            {
                if (_hubContext == null)
                {
                    _hubContext = _connectionManager.GetHubContext<Chat>();
                }

                return _hubContext;
            }
        }
    }
}