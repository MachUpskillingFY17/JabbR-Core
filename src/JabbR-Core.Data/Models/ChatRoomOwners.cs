using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatRoomOwners
    {
        public int ChatRoomKey { get; set; }
        public string ChatUserId { get; set; }

        public ChatRoom ChatRoomKeyNavigation { get; set; }
        public ChatUser ChatUserKeyNavigation { get; set; }
    }
}
