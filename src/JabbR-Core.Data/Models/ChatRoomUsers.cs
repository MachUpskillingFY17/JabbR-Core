using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatRoomUsers
    {
        public int ChatUserKey { get; set; }
        public int ChatRoomKey { get; set; }
        
        // TODO: combine all "join" tables/ classes into one table/ class with booleans to distinguish the relationships
        // Maybe make these and enum instead of multiple bools
        /*public bool Allowed { get; set; }
        public bool Owner { get; set; }*/

        public ChatRoom ChatRoomKeyNavigation { get; set; }
        public ChatUser ChatUserKeyNavigation { get; set; }
    }
}
