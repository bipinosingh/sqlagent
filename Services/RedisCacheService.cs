using StackExchange.Redis;

namespace SqlAgent.Services
{
    public class RedisCacheService
    {

        private readonly IDatabase _database;

        public RedisCacheService(
            IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<string?> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task SetAsync(
            string key,
            string value,
            TimeSpan expiry)
        {
            await _database.StringSetAsync(
                key,
                value,
                expiry);
        }

    }
}
