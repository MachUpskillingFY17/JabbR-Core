using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class Attachment
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

        public ChatUser OwnerKeyNavigation { get; set; }
        public ChatRoom RoomKeyNavigation { get; set; }
    }
}
