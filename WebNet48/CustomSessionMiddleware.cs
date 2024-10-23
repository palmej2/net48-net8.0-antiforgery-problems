using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using SameSiteMode = System.Web.SameSiteMode;

namespace WebNet48
{
    public class CustomSessionMiddleware
    {
        private AppFunc m_next;

        public CustomSessionMiddleware(AppFunc next)
        {
            m_next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);
            var sessionId = context.Request.Cookies["BC.SessionId"];

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                var cookie = new HttpCookie("BC.SessionId", sessionId) { HttpOnly = true, SameSite = SameSiteMode.Strict};
                HttpContext.Current.Response.Cookies.Add(cookie);

                //Generate Session in Redis
                RedisSessionManager redisSessionManager = new RedisSessionManager();
                await redisSessionManager.SetAsync(sessionId, "SessionCreated", DateTime.UtcNow.ToString());
            }

            HttpContext.Current.Items["SessionId"] = sessionId;

            await m_next(environment);
        }
    }
}