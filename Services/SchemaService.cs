using Dapper;
using SqlAgent.Data;

namespace SqlAgent.Services;

public class SchemaService
{
    private readonly DbConnectionFactory _dbFactory;

    public SchemaService(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<string> GetSchemaAsync()
    {
        var sql = @"
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        ORDER BY TABLE_NAME, ORDINAL_POSITION";

        using var connection = _dbFactory.CreateConnection();
        var result = await connection.QueryAsync(sql);

        var grouped = result
            .GroupBy(r => (string)r.TABLE_NAME)
            .Select(g =>
                $"{g.Key}({string.Join(", ", g.Select(c => (string)c.COLUMN_NAME))})"
            );

        return string.Join("\n", grouped);
    }
}