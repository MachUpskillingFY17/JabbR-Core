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
            AllowedUsers = new HashSet<ChatRoomChatUserAllowed>();
            Owners = new HashSet<ChatRoomChatUserOwner>();
            Users = new HashSet<ChatUserChatRooms>();
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
        public ICollection<ChatRoomChatUserAllowed> AllowedUsers { get; set; }
        public ICollection<ChatRoomChatUserOwner> Owners { get; set; }
        public ICollection<ChatUserChatRooms> Users { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ChatUser CreatorKeyNavigation { get; set; }
    }
}
