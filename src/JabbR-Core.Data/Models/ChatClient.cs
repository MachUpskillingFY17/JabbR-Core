using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatClient
    {
        public int Key { get; set; }
        public string Id { get; set; }
        public int UserKey { get; set; }
        public string UserAgent { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public string Name { get; set; }
        public DateTimeOffset LastClientActivity { get; set; }

        public ChatUser UserKeyNavigation { get; set; }
    }
}
