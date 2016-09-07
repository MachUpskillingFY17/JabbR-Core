using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatUserChatRooms
    {
        public int ChatUserKey { get; set; }
        public int ChatRoomKey { get; set; }

        public ChatRooms ChatRoomKeyNavigation { get; set; }
        public ChatUsers ChatUserKeyNavigation { get; set; }
    }
}
