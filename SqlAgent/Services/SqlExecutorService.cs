using Dapper;
using SqlAgent.Data;

namespace SqlAgent.Services;

public class SqlExecutorService
{
    private readonly DbConnectionFactory _dbFactory;

    public SqlExecutorService(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sql)
    {
        using var connection = _dbFactory.CreateConnection();
        var result = await connection.QueryAsync(sql);
        return result;
    }
}