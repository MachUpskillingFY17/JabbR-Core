using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatRooms
    {
        public ChatRooms()
        {
            Attachments = new HashSet<Attachments>();
            ChatMessages = new HashSet<ChatMessages>();
            AllowedUsers = new HashSet<ChatRoomChatUser1>();
            Owners = new HashSet<ChatRoomChatUsers>();
            Users = new HashSet<ChatUserChatRooms>();
            Notifications = new HashSet<Notifications>();
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

        public ICollection<Attachments> Attachments { get; set; }
        public ICollection<ChatMessages> ChatMessages { get; set; }
        public ICollection<ChatRoomChatUser1> AllowedUsers { get; set; }
        public ICollection<ChatRoomChatUsers> Owners { get; set; }
        public ICollection<ChatUserChatRooms> Users { get; set; }
        public ICollection<Notifications> Notifications { get; set; }
        public ChatUsers CreatorKeyNavigation { get; set; }
    }
}
