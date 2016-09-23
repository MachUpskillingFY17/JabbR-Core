﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JabbR_Core.Infrastructure;

namespace JabbR_Core.Models
{
    public class ChatMessage
    {
        [Key]
        public int Key { get; set; }

        public string Content { get; set; }
        public string Id { get; set; }
        public virtual ChatRoom Room { get; set; }
        public virtual ChatUser User { get; set; }
        public DateTimeOffset When { get; set; }
        public bool HtmlEncoded { get; set; }
        public int MessageType { get; set; }

        // After content providers run this is updated with the content
        public string HtmlContent { get; set; }

        public int? RoomKey { get; set; }
        public int? UserKey { get; set; }

        // Notifications
        public string ImageUrl { get; set; }
        public string Source { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }

        public ChatMessage()
        {
            Notifications = new SafeCollection<Notification>();
        }
    }
}