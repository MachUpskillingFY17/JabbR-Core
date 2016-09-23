using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Services
{
    public interface IEmailService
    {
        void SendRequestResetPassword(Models.ChatUser user, string siteBaseUrl);
    }
}
