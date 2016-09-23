using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace JabbR_Core.Controllers
{

    public class LoginFakerMiddleware
    {
        private readonly RequestDelegate _next;
        public LoginFakerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.Name == null)
            {
                var identities = new List<ClaimsIdentity>() { new ClaimsIdentity() { } };
                var principal = new ClaimsPrincipal(identities);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, "Jane"));
                claims.Add(new Claim(ClaimTypes.AuthenticationMethod, "provider"));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "identity"));
                claims.Add(new Claim(ClaimTypes.Email, "jane@no.com"));
                claims.Add(new Claim(JabbRClaimTypes.Identifier, "1"));
                var claimsIdentity = new ClaimsIdentity(claims, Constants.JabbRAuthType);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await context.Authentication.SignInAsync(Constants.JabbRAuthType, claimsPrincipal);
            }
            await _next.Invoke(context);
        }
    }
}
