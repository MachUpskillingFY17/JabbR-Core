using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class Notification
    {
        public int Key { get; set; }
        public int UserKey { get; set; }
        public int MessageKey { get; set; }
        public bool Read { get; set; }
        public int RoomKey { get; set; }

        public ChatMessage MessageKeyNavigation { get; set; }
        public ChatRoom RoomKeyNavigation { get; set; }
        public ChatUser UserKeyNavigation { get; set; }
    }
}
