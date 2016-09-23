using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatUserChatRooms
    {
        public int ChatUserKey { get; set; }
        public int ChatRoomKey { get; set; }
        
        // TODO: combine all "join" tables/ classes into one with booleans to distinguish the relationships
        /*public bool Allowed { get; set; }
        public bool Owner { get; set; }*/

        public ChatRoom ChatRoomKeyNavigation { get; set; }
        public ChatUser ChatUserKeyNavigation { get; set; }
    }
}
