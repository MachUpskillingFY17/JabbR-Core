using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatUserChatRooms
    {
        public int ChatUserKey { get; set; }
        public int ChatRoomKey { get; set; }

        public virtual ChatRooms ChatRoomKeyNavigation { get; set; }
        public virtual ChatUsers ChatUserKeyNavigation { get; set; }
    }
}
