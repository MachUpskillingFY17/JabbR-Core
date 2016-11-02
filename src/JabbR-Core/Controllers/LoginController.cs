using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace JabbR_Core.Controllers
{

    public class LoginController : Controller
    {
        private IHttpContextAccessor _context;
        public LoginController(IHttpContextAccessor context)
        {
            _context = context;
        }
        // GET: /<controller>/
        public async Task<IActionResult> Login()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, "Jane"));
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, "provider"));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "identity"));
            claims.Add(new Claim(ClaimTypes.Email, "jane@no.com"));
            claims.Add(new Claim(JabbRClaimTypes.Identifier, "1"));
            var claimsIdentity = new ClaimsIdentity(claims, Constants.JabbRAuthType);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await _context.HttpContext.Authentication.SignInAsync(Constants.JabbRAuthType, claimsPrincipal);
            return Content("Logged in");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync(Constants.JabbRAuthType);
            return Content("Logged out");
        }
    }
}
