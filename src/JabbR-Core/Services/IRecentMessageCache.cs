using System.Collections.Generic;
using JabbR_Core.Models;
using JabbR_Core.ViewModels;

namespace JabbR_Core.Services
{
    public interface IRecentMessageCache
    {
        void Add(ChatMessage message);

        void Add(string room, ICollection<MessageViewModel> messages);

        ICollection<MessageViewModel> GetRecentMessages(string roomName);
    }
}
