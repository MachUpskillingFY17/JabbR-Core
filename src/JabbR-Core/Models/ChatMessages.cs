using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatMessages
    {
        public ChatMessages()
        {
            Notifications = new HashSet<Notifications>();
        }

        public int Key { get; set; }
        public string Content { get; set; }
        public string Id { get; set; }
        public DateTimeOffset When { get; set; }
        public int? RoomKey { get; set; }
        public int? UserKey { get; set; }
        public bool HtmlEncoded { get; set; }
        public string HtmlContent { get; set; }
        public string ImageUrl { get; set; }
        public string Source { get; set; }
        public int MessageType { get; set; }

        public virtual ICollection<Notifications> Notifications { get; set; }
        public virtual ChatRooms RoomKeyNavigation { get; set; }
        public virtual ChatUsers UserKeyNavigation { get; set; }
    }
}
