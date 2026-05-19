namespace SqlAgent.Services
{
    public class SqlValidator
    {
        public bool IsSafe(string sql)
        {
            var upperSql = sql.ToUpper();

            // Must start with SELECT
            if (!upperSql.StartsWith("SELECT"))
                return false;

            // Block dangerous keywords
            var forbidden = new[] { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE" };

            return !forbidden.Any(f => upperSql.Contains(f));

        }

    }
}
