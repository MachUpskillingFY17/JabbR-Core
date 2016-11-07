using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatMessage
    {
        public ChatMessage()
        {
            Notifications = new HashSet<Notification>();
        }

        public int Key { get; set; }
        public string Content { get; set; }
        public string Id { get; set; }
        public DateTimeOffset When { get; set; }
        public int? RoomKey { get; set; }
        public string UserId { get; set; }
        public bool HtmlEncoded { get; set; }
        public string HtmlContent { get; set; }
        public string ImageUrl { get; set; }
        public string Source { get; set; }
        public int MessageType { get; set; }

        public ICollection<Notification> Notifications { get; set; }
        public ChatRoom RoomKeyNavigation { get; set; }
        public ChatUser UserKeyNavigation { get; set; }
    }
}
