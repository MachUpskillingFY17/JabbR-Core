using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatRoomChatUser1
    {
        public int ChatRoomKey { get; set; }
        public int ChatUserKey { get; set; }

        public ChatRooms ChatRoomKeyNavigation { get; set; }
        public ChatUsers ChatUserKeyNavigation { get; set; }
    }
}
