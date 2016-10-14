using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace JabbR_Core.Data.Models
{
    public partial class ChatUser : IdentityUser
    {
        public ChatUser()
        {
            Attachments = new HashSet<Attachment>();
            ConnectedClients = new HashSet<ChatClient>();
            ChatMessages = new HashSet<ChatMessage>();
            AllowedRooms = new HashSet<ChatPrivateRoomUsers>();
            OwnedRooms = new HashSet<ChatRoomOwners>();
            ChatRooms = new HashSet<ChatRoom>();
            Rooms = new HashSet<ChatRoomUsers>();
            ChatUserIdentities = new HashSet<ChatUserIdentity>();
            Notifications = new HashSet<Notification>();
        }

        //public int Key { get; set; }
        //public string Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime? LastNudged { get; set; }
        public int Status { get; set; }
        //public string HashedPassword { get; set; }
        //public string Salt { get; set; }
        public string Note { get; set; }
        public string AfkNote { get; set; }
        public bool IsAfk { get; set; }
        public string Flag { get; set; }
        public string Identity { get; set; }
        //public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public string RequestPasswordResetId { get; set; }
        public DateTimeOffset? RequestPasswordResetValidThrough { get; set; }
        public string RawPreferences { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
        public ICollection<ChatClient> ConnectedClients { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; }
        public ICollection<ChatPrivateRoomUsers> AllowedRooms { get; set; }
        public ICollection<ChatRoomOwners> OwnedRooms { get; set; }
        public ICollection<ChatRoom> ChatRooms { get; set; }
        public ICollection<ChatRoomUsers> Rooms { get; set; }
        public ICollection<ChatUserIdentity> ChatUserIdentities { get; set; }
        public ICollection<Notification> Notifications { get; set; }

        public bool HasUserNameAndPasswordCredentials()
        {
            return !String.IsNullOrEmpty(PasswordHash) && !String.IsNullOrEmpty(Name);
        }

        [NotMapped]
        public ChatUserPreferences Preferences
        {
            get
            {
                return ChatUserPreferences.GetPreferences(this);
            }

            set
            {
                value.Serialize(this);
            }
        }
    }
}
