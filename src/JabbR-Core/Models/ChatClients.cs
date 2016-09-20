using System;
using System.Collections.Generic;

namespace JabbR_Core.Models
{
    public partial class ChatClients
    {
        public int Key { get; set; }
        public string Id { get; set; }
        public int UserKey { get; set; }
        public string UserAgent { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public string Name { get; set; }
        public DateTimeOffset LastClientActivity { get; set; }

        public ChatUsers UserKeyNavigation { get; set; }
    }
}
