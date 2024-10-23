namespace WebNet48Core
{
    public class CustomSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisSessionManager _sessionManager;

        public CustomSessionMiddleware(RequestDelegate next)
        {
            _next = next;
            //_sessionManager = sessionManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get session ID from the cookie
            var sessionId = context.Request.Cookies["BC.SessionId"];

            if (string.IsNullOrEmpty(sessionId))
            {
                // Generate a new session ID
                sessionId = Guid.NewGuid().ToString();
                context.Response.Cookies.Append("BC.SessionId", sessionId, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });

                //Generate Session in Redis
                RedisSessionManager redisSessionManager = new RedisSessionManager();
                await redisSessionManager.SetAsync(sessionId, "SessionCreated", DateTime.UtcNow.ToString());
            }

            // Store session ID in context for easy access
            context.Items["SessionId"] = sessionId;

            // Process the request
            await _next(context);
        }
    }
}
