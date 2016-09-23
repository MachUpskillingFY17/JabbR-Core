using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Services;
using JabbR_Core.Data.Models;
using JabbR_Core.Data.Repositories;

namespace JabbR_Core.Tests.Services
{
    public class ChatServiceTest : IChatService
    {
        ChatService chatService;
        IJabbrRepository _repository;
        ICache _cache;
        IRecentMessageCache _recentMessageCache;

        public ChatServiceTest()
        {
            //instantiate _cache, etc here

            chatService = new ChatService(_cache, _recentMessageCache, _repository);
        }


        //TODO: write tests for each of these functions. 
        public void AddAdmin(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public ChatClient AddClient(ChatUser user, string clientId, string userAgent)
        {
            throw new NotImplementedException();
        }

        public ChatMessage AddMessage(string userId, string roomName, string url)
        {
            throw new NotImplementedException();
        }

        public ChatMessage AddMessage(ChatUser user, ChatRoom room, string id, string content)
        {
            throw new NotImplementedException();
        }

        public void AddNotification(ChatUser mentionedUser, ChatMessage message, ChatRoom room, bool markAsRead)
        {
            throw new NotImplementedException();
        }

        public void AddOwner(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public ChatRoom AddRoom(ChatUser user, string roomName)
        {
            throw new NotImplementedException();
        }

        public void AllowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void AppendMessage(string id, string content)
        {
            throw new NotImplementedException();
        }

        public void BanUser(ChatUser callingUser, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public void ChangeTopic(ChatUser user, ChatRoom room, string newTopic)
        {
            throw new NotImplementedException();
        }

        public void ChangeWelcome(ChatUser user, ChatRoom room, string newWelcome)
        {
            throw new NotImplementedException();
        }

        public void CloseRoom(ChatUser user, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public string DisconnectClient(string clientId)
        {
            throw new NotImplementedException();
        }

        public void JoinRoom(ChatUser user, ChatRoom room, string inviteCode)
        {
            throw new NotImplementedException();
        }

        public void KickUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void LeaveRoom(ChatUser user, ChatRoom room)
        {
            throw new NotImplementedException();
        }

        public void LockRoom(ChatUser user, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void OpenRoom(ChatUser user, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void RemoveAdmin(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public void RemoveOwner(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void SetInviteCode(ChatUser user, ChatRoom room, string inviteCode)
        {
            throw new NotImplementedException();
        }

        public void UnallowUser(ChatUser user, ChatUser targetUser, ChatRoom targetRoom)
        {
            throw new NotImplementedException();
        }

        public void UnbanUser(ChatUser admin, ChatUser targetUser)
        {
            throw new NotImplementedException();
        }

        public void UpdateActivity(ChatUser user, string clientId, string userAgent)
        {
            throw new NotImplementedException();
        }
    }
}
