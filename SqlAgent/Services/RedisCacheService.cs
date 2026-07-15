using StackExchange.Redis;

namespace SqlAgent.Services
{
    public class RedisCacheService
    {
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IConnectionMultiplexer redisConnectionMultiplexer;

        public RedisCacheService(
            IConnectionMultiplexer _redisConnectionMultiplexer, ILogger<RedisCacheService> logger)
        {
            //_database = redis.GetDatabase();
            _logger = logger;
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
        }

        public async Task<string?> GetAsync(string key)
        {
            try
            {
                var db = redisConnectionMultiplexer.GetDatabase();
                return await db.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Redis GET failed for key {Key}. Falling back to SQL Server.",
                    key);

                return null;
            }
        }

        public async Task SetAsync(
            string key,
            string value,
            TimeSpan expiry)
        {
            try
            {
                var db = redisConnectionMultiplexer.GetDatabase();

                await db.StringSetAsync(
                    key,
                    value,
                    expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Redis SET failed for key {Key}. Cache not updated.",
                    key);
            }
        }

    }
}
