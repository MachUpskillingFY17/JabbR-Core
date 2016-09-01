using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatRoomChatUsers
    {
        public int ChatRoomKey { get; set; }
        public int ChatUserKey { get; set; }

        public virtual ChatRooms ChatRoomKeyNavigation { get; set; }
        public virtual ChatUsers ChatUserKeyNavigation { get; set; }
    }
}
