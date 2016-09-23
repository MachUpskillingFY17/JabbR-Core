using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.Services
{
    public interface IUserAuthenticator
    {
        bool TryAuthenticateUser(string username, string password, out IList<Claim> claims);
    }
}
