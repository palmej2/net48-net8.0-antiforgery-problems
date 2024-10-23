using System.Threading.Tasks;
using System.Web;

namespace WebNet48
{
    public class SessionHelper
    {
        private readonly RedisSessionManager _sessionManager;
        private readonly HttpContext _context;

        public SessionHelper(RedisSessionManager sessionManager, HttpContext context)
        {
            _sessionManager = sessionManager;
            _context = context;
        }

        public async Task<string> GetAsync(string key)
        {
            var sessionId = _context.Items["SessionId"].ToString();
            return await _sessionManager.GetAsync(sessionId, key);
        }

        public async Task SetAsync(string key, string value)
        {
            var sessionId = _context.Items["SessionId"].ToString();
            await _sessionManager.SetAsync(sessionId, key, value);
        }
    }
}