using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class Attachments
    {
        public int Key { get; set; }
        public string Url { get; set; }
        public string Id { get; set; }
        public int RoomKey { get; set; }
        public int OwnerKey { get; set; }
        public DateTimeOffset When { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }

        public virtual ChatUsers OwnerKeyNavigation { get; set; }
        public virtual ChatRooms RoomKeyNavigation { get; set; }
    }
}
