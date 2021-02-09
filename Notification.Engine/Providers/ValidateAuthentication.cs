using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Engine.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidateAuthentication : IMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User == null || context.User.Identity == null)
            {
                await context.ChallengeAsync();
            }

            if (context.User.Identity.IsAuthenticated)
            {
                await next(context);
            }
            else
            {
                await context.ChallengeAsync();
            }
        }
    }
}
