using SqlAgent.Controllers;

namespace SqlAgent.Services
{
    public class SqlValidator
    {        
        private readonly ILogger<SqlValidator> _logger;
        public SqlValidator(ILogger<SqlValidator> logger) {
            _logger = logger;
        }
        public bool IsSafe(string sql)
        {
            _logger.LogInformation("SQL which Validator got :: {0}", sql);

            if (String.IsNullOrWhiteSpace(sql)) {
                return false;
            }

            sql = sql.Trim();
            _logger.LogInformation("SQL after Trimming :: {0}", sql);
            if (sql.EndsWith(";")) {
                _logger.LogInformation("SQL Ends with ; :: {0}", sql);
                sql = sql.TrimEnd(';');
                _logger.LogInformation("SQL After Trimmimg with ; :: {0}", sql);
            }

            var upperSql = sql.ToUpper();
            _logger.LogInformation("SQL after ToUpper :: {0}", upperSql);

            // Must start with SELECT
            if (!upperSql.StartsWith("SELECT"))
                return false;

            if (upperSql.Contains(";"))
                return false;

            // Block dangerous keywords
            var forbidden = new[] { "DROP ", "DELETE ", "UPDATE ", "INSERT ", "ALTER ", "TRUNCATE " };

            //return !forbidden.Any(f => upperSql.Contains(f));

            foreach (var f in forbidden)
            {
                if (upperSql.Contains(f))
                {
                    Console.WriteLine($"Blocked by keyword: {f}");
                    return false;
                }
            }

            return true;
        }

    }
}
