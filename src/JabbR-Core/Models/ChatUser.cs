using System;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JabbR_Core.Models
{
    public class ChatUser
    {
        [Key]
        public int Key { get; set; }
        [MaxLength(200)]
        public string Id { get; set; }
        public string Name { get; set; }
        // MD5 email hash for gravatar
        public string Hash { get; set; }
        public string Salt { get; set; }
        public string HashedPassword { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime? LastNudged { get; set; }
        public int Status { get; set; }
        [StringLength(200)]
        public string Note { get; set; }
        [StringLength(200)]
        public string AfkNote { get; set; }
        public bool IsAfk { get; set; }
        [StringLength(255)]
        public string Flag { get; set; }

        // TODO: Migrate everyone off identity and email
        public string Identity { get; set; }

        
        public string Email { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }

        // Request password reset token
        public string RequestPasswordResetId { get; set; }
        public DateTimeOffset? RequestPasswordResetValidThrough { get; set; }

        public string RawPreferences { get; set; }

        // List of clients that are currently connected for this user
        public ICollection<ChatUserIdentity> Identities { get; set; }
        public ICollection<ChatClient> ConnectedClients { get; set; }
        public ICollection<ChatRoom> OwnedRooms { get; set; }
        public ICollection<ChatRoom> Rooms { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
        public ICollection<Notification> Notifications { get; set; }

        // Private rooms this user is allowed to go into
        public ICollection<ChatRoom> AllowedRooms { get; set; }

        public ChatUser()
        {
            Identities = new SafeCollection<ChatUserIdentity>();
            ConnectedClients = new SafeCollection<ChatClient>();
            OwnedRooms = new SafeCollection<ChatRoom>();
            Rooms = new SafeCollection<ChatRoom>();
            AllowedRooms = new SafeCollection<ChatRoom>();
            Attachments = new SafeCollection<Attachment>();
            Notifications = new SafeCollection<Notification>();
        }

        public bool HasUserNameAndPasswordCredentials()
        {
            return !String.IsNullOrEmpty(HashedPassword) && !String.IsNullOrEmpty(Name);
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
