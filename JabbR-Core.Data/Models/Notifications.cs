using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class Notifications
    {
        public int Key { get; set; }
        public int UserKey { get; set; }
        public int MessageKey { get; set; }
        public bool Read { get; set; }
        public int RoomKey { get; set; }

        public ChatMessages MessageKeyNavigation { get; set; }
        public ChatRooms RoomKeyNavigation { get; set; }
        public ChatUsers UserKeyNavigation { get; set; }
    }
}
