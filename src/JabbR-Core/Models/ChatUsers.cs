using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatUsers
    {
        public ChatUsers()
        {
            Attachments = new HashSet<Attachments>();
            ChatClients = new HashSet<ChatClients>();
            ChatMessages = new HashSet<ChatMessages>();
            AllowedRooms = new HashSet<ChatRoomChatUser1>();
            OwnedRooms = new HashSet<ChatRoomChatUsers>();
            ChatRooms = new HashSet<ChatRooms>();
            Rooms = new HashSet<ChatUserChatRooms>();
            ChatUserIdentities = new HashSet<ChatUserIdentities>();
            Notifications = new HashSet<Notifications>();
        }

        public int Key { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime? LastNudged { get; set; }
        public int Status { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        public string Note { get; set; }
        public string AfkNote { get; set; }
        public bool IsAfk { get; set; }
        public string Flag { get; set; }
        public string Identity { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public string RequestPasswordResetId { get; set; }
        public DateTimeOffset? RequestPasswordResetValidThrough { get; set; }
        public string RawPreferences { get; set; }

        public ICollection<Attachments> Attachments { get; set; }
        public ICollection<ChatClients> ChatClients { get; set; }
        public ICollection<ChatMessages> ChatMessages { get; set; }
        public ICollection<ChatRoomChatUser1> AllowedRooms { get; set; }
        public ICollection<ChatRoomChatUsers> OwnedRooms { get; set; }
        public ICollection<ChatRooms> ChatRooms { get; set; }
        public ICollection<ChatUserChatRooms> Rooms { get; set; }
        public ICollection<ChatUserIdentities> ChatUserIdentities { get; set; }
        public ICollection<Notifications> Notifications { get; set; }
    }
}
