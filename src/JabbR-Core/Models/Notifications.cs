using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class Notifications
    {
        public int Key { get; set; }
        public int UserKey { get; set; }
        public int MessageKey { get; set; }
        public bool Read { get; set; }
        public int RoomKey { get; set; }

        public virtual ChatMessages MessageKeyNavigation { get; set; }
        public virtual ChatRooms RoomKeyNavigation { get; set; }
        public virtual ChatUsers UserKeyNavigation { get; set; }
    }
}
