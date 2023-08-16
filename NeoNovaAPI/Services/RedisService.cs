using StackExchange.Redis;

namespace NeoNovaAPI.Services
{
    public class RedisService
    {
        private readonly IDatabase _cache;

        public RedisService(ConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        public string GetString(string key) => _cache.StringGet(key);

        public void SetString(string key, string value, TimeSpan? expiry = null) => _cache.StringSet(key, value, expiry);

        public void DeleteKey(string key) => _cache.KeyDelete(key);
    }

}
