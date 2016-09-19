using System;
using System.Collections.Generic;

namespace JabbR_Core.Data.Models
{
    public partial class ChatUserIdentity
    {
        public int Key { get; set; }
        public int UserKey { get; set; }
        public string Email { get; set; }
        public string Identity { get; set; }
        public string ProviderName { get; set; }

        public ChatUser UserKeyNavigation { get; set; }
    }
}
