using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace WebNet48
{
    public class RedisSessionManager
    {
        private readonly IDatabase _redisDb;
        private readonly string _sessionKeyPrefix = "BC.Session_"; // Prefix to keep sessions unique
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30); // Customize as needed

        public RedisSessionManager()
        {
            var redisConnectionString = ConfigurationManager.AppSettings["RedisConnectionString"];
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _redisDb = redis.GetDatabase();
        }

        // Generate session key based on the session ID
        private string GetRedisKey(string sessionId) => $"{_sessionKeyPrefix}{sessionId}";

        // Get a value from session by key
        public async Task<string> GetAsync(string sessionId, string key)
        {
            var redisKey = GetRedisKey(sessionId);
            return await _redisDb.HashGetAsync(redisKey, key);
        }

        // Set a value in session by key
        public async Task SetAsync(string sessionId, string key, string value)
        {
            var redisKey = GetRedisKey(sessionId);
            await _redisDb.HashSetAsync(redisKey, key, value);

            // Extend session timeout
            await _redisDb.KeyExpireAsync(redisKey, _sessionTimeout);
        }

        // Get all session data as a Dictionary
        public async Task<Dictionary<string, string>> GetAllSessionDataAsync(string sessionId)
        {
            var redisKey = GetRedisKey(sessionId);
            var entries = await _redisDb.HashGetAllAsync(redisKey);

            var sessionData = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                sessionData[entry.Name] = entry.Value;
            }

            return sessionData;
        }

        // Save session data into Redis (if you're manipulating a full dictionary)
        public async Task SaveSessionDataAsync(string sessionId, Dictionary<string, string> sessionData)
        {
            var redisKey = GetRedisKey(sessionId);

            foreach (var kvp in sessionData)
            {
                await _redisDb.HashSetAsync(redisKey, kvp.Key, kvp.Value);
            }

            // Set session timeout
            await _redisDb.KeyExpireAsync(redisKey, _sessionTimeout);
        }

        // Remove session data from Redis
        public async Task RemoveSessionAsync(string sessionId)
        {
            var redisKey = GetRedisKey(sessionId);
            await _redisDb.KeyDeleteAsync(redisKey);
        }
    }
}