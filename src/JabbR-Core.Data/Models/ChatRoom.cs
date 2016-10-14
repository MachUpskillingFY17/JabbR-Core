using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatRoom
    {
        public ChatRoom()
        {
            Attachments = new HashSet<Attachment>();
            ChatMessages = new HashSet<ChatMessage>();
            AllowedUsers = new HashSet<ChatPrivateRoomUsers>();
            Owners = new HashSet<ChatRoomOwners>();
            Users = new HashSet<ChatRoomUsers>();
            Notifications = new HashSet<Notification>();
        }

        public int Key { get; set; }
        public DateTime? LastNudged { get; set; }
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public bool Private { get; set; }
        public string InviteCode { get; set; }
        public bool Closed { get; set; }
        public string Topic { get; set; }
        public string Welcome { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; }
        public ICollection<ChatPrivateRoomUsers> AllowedUsers { get; set; }
        public ICollection<ChatRoomOwners> Owners { get; set; }
        public ICollection<ChatRoomUsers> Users { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ChatUser CreatorKeyNavigation { get; set; }
    }
}
