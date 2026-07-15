using Dapper;
using SqlAgent.Data;

namespace SqlAgent.Services;

public class SchemaService
{
    private readonly DbConnectionFactory _dbFactory;
    private readonly RedisCacheService _cache;
    private readonly ILogger<SchemaService> _logger;

    public SchemaService(DbConnectionFactory dbFactory,
                        RedisCacheService cache,
                        ILogger<SchemaService> logger)
    {
        _dbFactory = dbFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<string>> GetColumnsAsync()
    {
        var sql = @"
        SELECT COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS";

        using var connection = _dbFactory.CreateConnection();

        var result = await connection.QueryAsync<string>(sql);

        return result.ToList();
    }

    public async Task<string> GetSchemaAsync()
    {
        const string cacheKey = "DatabaseSchema";

        // ----------------------------
        // Try to get schema from Redis
        // ----------------------------
        try
        {
            _logger.LogInformation("Checking schema in Redis cache for {0}", cacheKey);
            var cachedSchema = await _cache.GetAsync(cacheKey);

            if (!string.IsNullOrWhiteSpace(cachedSchema))
            {
                _logger.LogInformation("Database schema loaded from Redis cache key {0}", cacheKey);
                return cachedSchema;
            }

            _logger.LogInformation(
                "Schema not found in Redis cache.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Redis is unavailable. Falling back to SQL Server.");
        }

        // ----------------------------
        // Read schema from SQL Server
        // ----------------------------
        const string sql = @"
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        ORDER BY TABLE_NAME, ORDINAL_POSITION";

        using var connection = _dbFactory.CreateConnection();

        var result = await connection.QueryAsync(sql);

        var grouped = result
            .GroupBy(r => (string)r.TABLE_NAME)
            .Select(g =>
                $"{g.Key}({string.Join(", ", g.Select(c => (string)c.COLUMN_NAME))})");

        var schema = string.Join("\n", grouped);

        _logger.LogInformation(
            "Database schema loaded from SQL Server.");

        // ----------------------------
        // Try to cache it
        // ----------------------------
        try
        {
            await _cache.SetAsync(
                cacheKey,
                schema,
                TimeSpan.FromHours(1));

            _logger.LogInformation(
                "Database schema cached successfully in Redis.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unable to cache schema because Redis is unavailable.");
        }

        return schema;
    }
}