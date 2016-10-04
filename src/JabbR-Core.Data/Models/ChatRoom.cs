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
            AllowedUsers = new HashSet<UserRoomAllowed>();
            Owners = new HashSet<UserRoomOwner>();
            Users = new HashSet<UserRoom>();
            Notifications = new HashSet<Notification>();
        }

        public int Key { get; set; }
        public DateTime? LastNudged { get; set; }
        public string Name { get; set; }
        public int? CreatorKey { get; set; }
        public bool Private { get; set; }
        public string InviteCode { get; set; }
        public bool Closed { get; set; }
        public string Topic { get; set; }
        public string Welcome { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; }
        public ICollection<UserRoomAllowed> AllowedUsers { get; set; }
        public ICollection<UserRoomOwner> Owners { get; set; }
        public ICollection<UserRoom> Users { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ChatUser CreatorKeyNavigation { get; set; }
    }
}
