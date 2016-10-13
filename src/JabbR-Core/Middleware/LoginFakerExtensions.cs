using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Controllers;
using Microsoft.AspNetCore.Builder;

namespace JabbR_Core.Middleware
{
    public static class LoginFakerExtensions
    {
        public static IApplicationBuilder UseFakeLogin(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoginFakerMiddleware>();
        }

    }
}
